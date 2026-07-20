using System;
using System.Collections.Generic;
using System.Linq;

namespace PuckStats
{
    /// <summary>
    /// Core rating engine. Computes 0-100 ratings for all skill categories.
    /// Z-score normalization (mean=50, SD=15) against calibrated population stats.
    /// </summary>
    public class RatingEngine
    {
        private readonly PopulationStats _population;

        private static readonly Dictionary<string, double> OverallWeights = new()
        {
            ["Skating"] = 0.12, ["Shooting"] = 0.15, ["Stickhandling"] = 0.13, ["Passing"] = 0.10,
            ["Inputs"] = 0.05, ["StickMotion"] = 0.05, ["OffensivePlay"] = 0.12, ["DefensivePlay"] = 0.10,
            ["Positioning"] = 0.08, ["GameSense"] = 0.10,
        };

        public RatingEngine(PopulationStats population) => _population = population;

        public PlayerRatings ComputeRatings(PlayerTelemetryAggregate a)
        {
            var r = new PlayerRatings
            {
                Skating = ComputeSkatingRating(a), Shooting = ComputeShootingRating(a),
                Stickhandling = ComputeStickhandlingRating(a), Passing = ComputePassingRating(a),
                Inputs = ComputeInputRating(a), StickMotion = ComputeStickMotionRating(a),
                OffensivePlay = ComputeOffensiveRating(a), DefensivePlay = ComputeDefensiveRating(a),
                Positioning = ComputePositioningRating(a), GameSense = ComputeGameSenseRating(a),
            };
            r.Overall = ComputeOverall(r);
            r.Archetype = DetectArchetype(r);
            return r;
        }

        public int ComputeOverall(PlayerRatings r)
        {
            double w = 0;
            w += r.Skating * OverallWeights["Skating"] + r.Shooting * OverallWeights["Shooting"];
            w += r.Stickhandling * OverallWeights["Stickhandling"] + r.Passing * OverallWeights["Passing"];
            w += r.Inputs * OverallWeights["Inputs"] + r.StickMotion * OverallWeights["StickMotion"];
            w += r.OffensivePlay * OverallWeights["OffensivePlay"] + r.DefensivePlay * OverallWeights["DefensivePlay"];
            w += r.Positioning * OverallWeights["Positioning"] + r.GameSense * OverallWeights["GameSense"];
            return Clamp((int)Math.Round(w));
        }

        public Archetype DetectArchetype(PlayerRatings r)
        {
            var s = new Dictionary<string, int>
            {
                ["Sniper"] = r.Shooting + r.OffensivePlay,
                ["Playmaker"] = r.Passing + r.Stickhandling,
                ["Grinder"] = r.DefensivePlay + r.Inputs,
                ["Two-Way"] = (r.OffensivePlay + r.DefensivePlay) / 2,
                ["OffensiveD"] = r.Shooting + r.OffensivePlay + r.Skating,
                ["DefensiveD"] = r.DefensivePlay + r.Positioning + r.StickMotion,
                ["PuckPossession"] = r.Stickhandling + r.Passing + r.GameSense,
            };
            if (s.Values.Max() - s.Values.Min() < 15) return Archetype.Hybrid;
            return s.OrderByDescending(kv => kv.Value).First().Key switch
            {
                "Sniper" => Archetype.Sniper, "Playmaker" => Archetype.Playmaker,
                "Grinder" => Archetype.Grinder, "Two-Way" => Archetype.TwoWayForward,
                "OffensiveD" => Archetype.OffensiveDefenseman, "DefensiveD" => Archetype.DefensiveDefenseman,
                "PuckPossession" => Archetype.PuckPossessionSpecialist, _ => Archetype.Hybrid
            };
        }

        #region Category Ratings
        public int ComputeSkatingRating(PlayerTelemetryAggregate a)
        {
            double s = 50;
            s += Z(a.AvgTopSpeed, _population.TopSpeed) * 0.30;
            s += Z(a.AvgSpeed, _population.AvgSpeed) * 0.15;
            s += Z(a.AvgBurstSpeed, _population.BurstSpeed) * 0.15;
            s += Z(a.AvgDirectionChanges, _population.DirectionChanges) * 0.20;
            s += Z(a.DistancePerMinute, _population.DistancePerMinute) * 0.10;
            s += Z(a.SprintEfficiency, _population.SprintEfficiency) * 0.10;
            return Clamp((int)Math.Round(s));
        }
        public int ComputeShootingRating(PlayerTelemetryAggregate a)
        {
            double s = 50;
            s += Z(a.GoalConversionRate, _population.GoalConversion) * 0.30;
            s += Z(a.ShotAccuracy, _population.ShotAccuracy) * 0.25;
            s += Z(a.AvgShotSpeed, _population.ShotSpeed) * 0.15;
            s += Z(a.GoalsPerGame, _population.GoalsPerGame) * 0.20;
            s += Z(a.ShotDangerScore, _population.ShotDanger) * 0.10;
            return Clamp((int)Math.Round(s));
        }
        public int ComputeStickhandlingRating(PlayerTelemetryAggregate a)
        {
            double s = 50;
            s += Z(a.AvgPossessionTime, _population.PossessionTime) * 0.25;
            s += Z(a.PossessionEfficiency, _population.PossessionEfficiency) * 0.20;
            s += Z(-a.TurnoverRate, _population.TurnoverRate) * 0.25;
            s += Z(a.PuckTouchesPerGame, _population.PuckTouchesPerGame) * 0.15;
            s += Z(a.StickActivity, _population.StickActivity) * 0.15;
            return Clamp((int)Math.Round(s));
        }
        public int ComputePassingRating(PlayerTelemetryAggregate a)
        {
            double s = 50;
            s += Z(a.PassCompletionRate, _population.PassCompletion) * 0.30;
            s += Z(a.PassesPerGame, _population.PassesPerGame) * 0.15;
            s += Z(a.DangerousPassesPerGame, _population.DangerousPasses) * 0.25;
            s += Z(a.AssistsPerGame, _population.AssistsPerGame) * 0.20;
            s += Z(a.PassDirectionDiversity, _population.PassDirectionDiversity) * 0.10;
            return Clamp((int)Math.Round(s));
        }
        public int ComputeInputRating(PlayerTelemetryAggregate a)
        {
            double s = 50;
            s += Z(a.InputConsistency, _population.InputConsistency) * 0.25;
            s += Z(a.InputComplexity, _population.InputComplexity) * 0.20;
            s += Z(-a.AvgReactionMs, _population.ReactionMs) * 0.20;
            s += Z(a.ActionsPerMinute, _population.ActionsPerMinute) * 0.15;
            s += Z(a.StickInputEfficiency, _population.StickInputEfficiency) * 0.10;
            s += Z(a.WasdBalance, _population.WasdBalance) * 0.10;
            return Clamp((int)Math.Round(s));
        }
        public int ComputeStickMotionRating(PlayerTelemetryAggregate a)
        {
            double s = 50;
            s += Z(a.AvgStickSpeed, _population.StickSpeed) * 0.20;
            s += Z(a.StickAnglePrecision, _population.StickPrecision) * 0.20;
            s += Z(a.StickEfficiency, _population.StickEfficiency) * 0.30;
            s += Z(a.StickActivity, _population.StickActivity) * 0.15;
            s += Z(a.StickAngleRange, _population.StickAngleRange) * 0.15;
            return Clamp((int)Math.Round(s));
        }
        public int ComputeOffensiveRating(PlayerTelemetryAggregate a)
        {
            double s = 50;
            s += Z(a.OffensiveZoneTime, _population.OffensiveZoneTime) * 0.25;
            s += Z(a.ShotCreationRate, _population.ShotCreation) * 0.25;
            s += Z(a.ChanceCreationRate, _population.ChanceCreation) * 0.25;
            s += Z(a.OffensivePositioning, _population.OffPositioning) * 0.15;
            s += Z(a.GoalsPerGame, _population.GoalsPerGame) * 0.10;
            return Clamp((int)Math.Round(s));
        }
        public int ComputeDefensiveRating(PlayerTelemetryAggregate a)
        {
            double s = 50;
            s += Z(a.InterceptionsPerGame, _population.InterceptionsPerGame) * 0.25;
            s += Z(a.BlocksPerGame, _population.BlocksPerGame) * 0.20;
            s += Z(a.DefensivePositioning, _population.DefPositioning) * 0.20;
            s += Z(a.PuckRecoveriesPerGame, _population.RecoveriesPerGame) * 0.15;
            s += Z(a.ZoneCoverage, _population.ZoneCoverage) * 0.10;
            s += Z(a.FaceoffWinRate, _population.FaceoffWinRate) * 0.10;
            return Clamp((int)Math.Round(s));
        }
        public int ComputePositioningRating(PlayerTelemetryAggregate a)
        {
            double s = 50;
            s += Z(a.ZoneOccupancyConsistency, _population.ZoneOccupancyConsistency) * 0.25;
            s += Z(a.AvgSpacingToTeammates, _population.SpacingToTeammates) * 0.20;
            s += Z(-a.PositionVariance, _population.PositionVariance) * 0.25;
            s += Z(a.TeamSupportTime, _population.TeamSupportTime) * 0.20;
            s += Z(a.TransitionSpeed, _population.TransitionSpeed) * 0.10;
            return Clamp((int)Math.Round(s));
        }
        public int ComputeGameSenseRating(PlayerTelemetryAggregate a)
        {
            double s = 50;
            s += Z(a.WinRateAboveTeam, _population.WinRateAboveTeam) * 0.25;
            s += Z(a.DecisionQuality, _population.DecisionQuality) * 0.25;
            s += Z(a.AnticipationScore, _population.Anticipation) * 0.20;
            s += Z(a.TeamSynergy, _population.TeamSynergy) * 0.15;
            s += Z(a.ClutchScore, _population.ClutchScore) * 0.15;
            return Clamp((int)Math.Round(s));
        }
        #endregion

        public Percentiles ComputePercentiles(PlayerRatings r)
        {
            return new Percentiles
            {
                SkatingPercentile = ToPct(r.Skating), ShootingPercentile = ToPct(r.Shooting),
                StickhandlingPercentile = ToPct(r.Stickhandling), PassingPercentile = ToPct(r.Passing),
                InputsPercentile = ToPct(r.Inputs), StickMotionPercentile = ToPct(r.StickMotion),
                OffensivePlayPercentile = ToPct(r.OffensivePlay), DefensivePlayPercentile = ToPct(r.DefensivePlay),
                PositioningPercentile = ToPct(r.Positioning), GameSensePercentile = ToPct(r.GameSense),
                OverallPercentile = ToPct(r.Overall),
            };
        }

        public static int ToPct(int rating) => Clamp((int)Math.Round(NormalCdf((rating - 50.0) / 15.0) * 100));

        public ScoutingReport GenerateScoutingReport(PlayerRatings r, PlayerTelemetryAggregate a)
        {
            var report = new ScoutingReport
            {
                OverallRating = r.Overall, Archetype = r.Archetype,
                ArchetypeDescription = ArchetypeDesc(r.Archetype),
            };
            if (r.Skating >= 75) report.Strengths.Add("Elite skating ability — high speed and agility.");
            if (r.Shooting >= 75) report.Strengths.Add("Strong shooting accuracy and goal conversion.");
            if (r.Stickhandling >= 75) report.Strengths.Add("Excellent puck control and possession retention.");
            if (r.Passing >= 75) report.Strengths.Add("Precise passing and playmaking vision.");
            if (r.OffensivePlay >= 75) report.Strengths.Add("Strong offensive pressure and chance creation.");
            if (r.DefensivePlay >= 75) report.Strengths.Add("Solid defensive coverage and puck recoveries.");
            if (r.Positioning >= 75) report.Strengths.Add("Excellent positional awareness and spacing.");
            if (r.GameSense >= 75) report.Strengths.Add("High hockey IQ and decision making.");

            if (r.Shooting <= 35) report.Weaknesses.Add("Below-average shooting accuracy.");
            if (r.Skating <= 35) report.Weaknesses.Add("Below-average skating speed and endurance.");
            if (r.Stickhandling <= 35) report.Weaknesses.Add("Poor puck control — high turnover rate.");
            if (r.Passing <= 35) report.Weaknesses.Add("Inconsistent passing — low completion rate.");
            if (r.DefensivePlay <= 35) report.Weaknesses.Add("Weak defensive coverage and positioning.");
            if (r.Positioning <= 35) report.Weaknesses.Add("Poor positional discipline — often out of position.");

            if (report.Strengths.Count == 0) report.Strengths.Add("Balanced player with developing skills.");
            if (report.Weaknesses.Count == 0) report.Weaknesses.Add("No significant weaknesses identified.");

            if (r.Shooting < 40) report.ImprovementSuggestions.Add("Increase shot volume from high-danger areas.");
            if (r.DefensivePlay < 40) report.ImprovementSuggestions.Add("Improve defensive positioning — stay between opponent and net.");
            if (r.Stickhandling < 40) report.ImprovementSuggestions.Add("Reduce neutral-zone turnovers — use safer outlet passes.");
            if (r.Positioning < 40) report.ImprovementSuggestions.Add("Maintain optimal spacing from teammates in offensive zone.");

            report.Summary = r.Overall >= 90 ? "Elite player. A dominant force on the ice."
                : r.Overall >= 80 ? "Strong player with above-average abilities."
                : r.Overall >= 65 ? "Solid player with a well-rounded skill set."
                : r.Overall >= 50 ? "Average player with developing strengths."
                : r.Overall >= 35 ? "Developing player with potential."
                : "Learning the fundamentals. Consistent practice will yield improvement.";
            return report;
        }

        #region Helpers
        private static double Z(double value, StatMetric m) => Math.Abs(m.StdDev) < 0.001 ? 0 : (value - m.Mean) / m.StdDev * 15;
        private static int Clamp(int v) => v < 0 ? 0 : v > 100 ? 100 : v;

        private static double NormalCdf(double x)
        {
            int sign = x < 0 ? -1 : 1;
            x = Math.Abs(x) / Math.Sqrt(2.0);
            double t = 1.0 / (1.0 + 0.3275911 * x);
            double y = 1.0 - (((((1.061405429 * t - 1.453152027) * t + 1.421413741) * t - 0.284496736) * t + 0.254829592) * t * Math.Exp(-x * x));
            return 0.5 * (1.0 + sign * y);
        }

        private static string ArchetypeDesc(Archetype a) => a switch
        {
            Archetype.Sniper => "A pure goal scorer. Elite shooting with high conversion rates.",
            Archetype.Playmaker => "A creative passer with excellent vision and high assist totals.",
            Archetype.Grinder => "A hard-working defensive forward who wins puck battles.",
            Archetype.TwoWayForward => "A complete player contributing at both ends of the ice.",
            Archetype.OffensiveDefenseman => "A blue-liner who drives offense from the back end.",
            Archetype.DefensiveDefenseman => "A stay-at-home defender focused on preventing goals.",
            Archetype.PuckPossessionSpecialist => "A possession-dominant player controlling the puck.",
            Archetype.Hybrid => "A versatile player without a clearly defined specialty.",
            _ => "Player archetype not yet determined."
        };
        #endregion
    }

    public class PopulationStats
    {
        public StatMetric TopSpeed = new(12.5f, 3.2f), AvgSpeed = new(6.8f, 1.5f), BurstSpeed = new(14.2f, 2.8f);
        public StatMetric DirectionChanges = new(45f, 15f), DistancePerMinute = new(280f, 60f), SprintEfficiency = new(0.65f, 0.15f);
        public StatMetric GoalConversion = new(0.12f, 0.06f), ShotAccuracy = new(0.55f, 0.15f), ShotSpeed = new(75f, 12f);
        public StatMetric GoalsPerGame = new(0.8f, 0.5f), ShotDanger = new(0.35f, 0.15f);
        public StatMetric PossessionTime = new(120f, 30f), PossessionEfficiency = new(0.7f, 0.15f), TurnoverRate = new(3.5f, 1.5f);
        public StatMetric PuckTouchesPerGame = new(45f, 15f), StickActivity = new(60f, 20f);
        public StatMetric PassCompletion = new(0.72f, 0.12f), PassesPerGame = new(25f, 8f), DangerousPasses = new(2.5f, 1.2f);
        public StatMetric AssistsPerGame = new(0.6f, 0.4f), PassDirectionDiversity = new(0.6f, 0.15f);
        public StatMetric InputConsistency = new(0.7f, 0.15f), InputComplexity = new(0.55f, 0.2f), ReactionMs = new(250f, 60f);
        public StatMetric ActionsPerMinute = new(80f, 25f), StickInputEfficiency = new(0.6f, 0.15f), WasdBalance = new(0.7f, 0.15f);
        public StatMetric StickSpeed = new(120f, 30f), StickPrecision = new(0.65f, 0.15f), StickEfficiency = new(0.55f, 0.15f);
        public StatMetric StickAngleRange = new(0.6f, 0.2f);
        public StatMetric OffensiveZoneTime = new(0.3f, 0.1f), ShotCreation = new(0.25f, 0.1f), ChanceCreation = new(0.12f, 0.06f);
        public StatMetric OffPositioning = new(0.4f, 0.1f);
        public StatMetric InterceptionsPerGame = new(4f, 2f), BlocksPerGame = new(2f, 1.2f), DefPositioning = new(0.45f, 0.1f);
        public StatMetric RecoveriesPerGame = new(5f, 2.5f), ZoneCoverage = new(0.6f, 0.1f), FaceoffWinRate = new(0.48f, 0.1f);
        public StatMetric ZoneOccupancyConsistency = new(0.55f, 0.15f), SpacingToTeammates = new(0.4f, 0.1f);
        public StatMetric PositionVariance = new(2.5f, 1.2f), TeamSupportTime = new(0.5f, 0.1f), TransitionSpeed = new(0.6f, 0.15f);
        public StatMetric WinRateAboveTeam = new(0.02f, 0.08f), DecisionQuality = new(0.55f, 0.15f);
        public StatMetric Anticipation = new(0.5f, 0.15f), TeamSynergy = new(0.45f, 0.1f), ClutchScore = new(0.5f, 0.2f);
    }

    public struct StatMetric { public float Mean, StdDev; public StatMetric(float m, float s) { Mean = m; StdDev = s; } }

    public class PlayerTelemetryAggregate
    {
        public float AvgTopSpeed, AvgSpeed, AvgBurstSpeed, AvgDirectionChanges, DistancePerMinute, SprintEfficiency;
        public float GoalConversionRate, ShotAccuracy, AvgShotSpeed, GoalsPerGame, ShotDangerScore;
        public float AvgPossessionTime, PossessionEfficiency, TurnoverRate, PuckTouchesPerGame, StickActivity;
        public float PassCompletionRate, PassesPerGame, DangerousPassesPerGame, AssistsPerGame, PassDirectionDiversity;
        public float InputConsistency, InputComplexity, AvgReactionMs, ActionsPerMinute, StickInputEfficiency, WasdBalance;
        public float AvgStickSpeed, StickAnglePrecision, StickEfficiency, StickAngleRange;
        public float OffensiveZoneTime, ShotCreationRate, ChanceCreationRate, OffensivePositioning;
        public float InterceptionsPerGame, BlocksPerGame, DefensivePositioning, PuckRecoveriesPerGame, ZoneCoverage, FaceoffWinRate;
        public float ZoneOccupancyConsistency, AvgSpacingToTeammates, PositionVariance, TeamSupportTime, TransitionSpeed;
        public float WinRateAboveTeam, DecisionQuality, AnticipationScore, TeamSynergy, ClutchScore;

        public static PlayerTelemetryAggregate Neutral() => new()
        {
            AvgTopSpeed = 12.5f, AvgSpeed = 6.8f, AvgBurstSpeed = 14.2f, AvgDirectionChanges = 45f,
            DistancePerMinute = 280f, SprintEfficiency = 0.65f, GoalConversionRate = 0.12f, ShotAccuracy = 0.55f,
            AvgShotSpeed = 75f, GoalsPerGame = 0.8f, ShotDangerScore = 0.35f, AvgPossessionTime = 120f,
            PossessionEfficiency = 0.7f, TurnoverRate = 3.5f, PuckTouchesPerGame = 45f, StickActivity = 60f,
            PassCompletionRate = 0.72f, PassesPerGame = 25f, DangerousPassesPerGame = 2.5f, AssistsPerGame = 0.6f,
            PassDirectionDiversity = 0.6f, InputConsistency = 0.7f, InputComplexity = 0.55f, AvgReactionMs = 250f,
            ActionsPerMinute = 80f, StickInputEfficiency = 0.6f, WasdBalance = 0.7f, AvgStickSpeed = 120f,
            StickAnglePrecision = 0.65f, StickEfficiency = 0.55f, StickAngleRange = 0.6f, OffensiveZoneTime = 0.3f,
            ShotCreationRate = 0.25f, ChanceCreationRate = 0.12f, OffensivePositioning = 0.4f,
            InterceptionsPerGame = 4f, BlocksPerGame = 2f, DefensivePositioning = 0.45f, PuckRecoveriesPerGame = 5f,
            ZoneCoverage = 0.6f, FaceoffWinRate = 0.48f, ZoneOccupancyConsistency = 0.55f, AvgSpacingToTeammates = 0.4f,
            PositionVariance = 2.5f, TeamSupportTime = 0.5f, TransitionSpeed = 0.6f, WinRateAboveTeam = 0.02f,
            DecisionQuality = 0.55f, AnticipationScore = 0.5f, TeamSynergy = 0.45f, ClutchScore = 0.5f
        };
    }
}
