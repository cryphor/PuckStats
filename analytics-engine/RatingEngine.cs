using PuckStats.Shared;

namespace PuckStats.Analytics;

public class RatingEngine
{
    private static readonly Dictionary<string, double> OverallWeights = new()
    {
        ["Skating"] = 0.12, ["Shooting"] = 0.15, ["Stickhandling"] = 0.13, ["Passing"] = 0.10,
        ["Inputs"] = 0.05, ["StickMotion"] = 0.05, ["OffensivePlay"] = 0.12, ["DefensivePlay"] = 0.10,
        ["Positioning"] = 0.08, ["GameSense"] = 0.10,
    };

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
        return Math.Clamp((int)Math.Round(w), 0, 100);
    }

    public Archetype DetectArchetype(PlayerRatings r)
    {
        var s = new Dictionary<string, int>
        {
            ["Sniper"] = r.Shooting + r.OffensivePlay,
            ["Playmaker"] = r.Passing + r.Stickhandling,
            ["Grinder"] = r.DefensivePlay + r.Inputs,
            ["TwoWayForward"] = (r.OffensivePlay + r.DefensivePlay) / 2,
            ["OffensiveD"] = r.Shooting + r.OffensivePlay + r.Skating,
            ["DefensiveD"] = r.DefensivePlay + r.Positioning + r.StickMotion,
            ["PuckPossession"] = r.Stickhandling + r.Passing + r.GameSense,
        };
        return s.Values.Max() - s.Values.Min() < 15 ? Archetype.Hybrid :
            s.OrderByDescending(kv => kv.Value).First().Key switch
            {
                "Sniper" => Archetype.Sniper, "Playmaker" => Archetype.Playmaker,
                "Grinder" => Archetype.Grinder, "TwoWayForward" => Archetype.TwoWayForward,
                "OffensiveD" => Archetype.OffensiveDefenseman, "DefensiveD" => Archetype.DefensiveDefenseman,
                "PuckPossession" => Archetype.PuckPossessionSpecialist, _ => Archetype.Hybrid
            };
    }

    public int ComputeSkatingRating(PlayerTelemetryAggregate a) => Clamp(50 + Z(a.AvgTopSpeed, 12.5f, 3.2f) * 30 + Z(a.AvgSpeed, 6.8f, 1.5f) * 15 + Z(a.AvgBurstSpeed, 14.2f, 2.8f) * 15 + Z(a.AvgDirectionChanges, 45f, 15f) * 20 + Z(a.DistancePerMinute, 280f, 60f) * 10 + Z(a.SprintEfficiency, 0.65f, 0.15f) * 10);
    public int ComputeShootingRating(PlayerTelemetryAggregate a) => Clamp(50 + Z(a.GoalConversionRate, 0.12f, 0.06f) * 30 + Z(a.ShotAccuracy, 0.55f, 0.15f) * 25 + Z(a.AvgShotSpeed, 75f, 12f) * 15 + Z(a.GoalsPerGame, 0.8f, 0.5f) * 20 + Z(a.ShotDangerScore, 0.35f, 0.15f) * 10);
    public int ComputeStickhandlingRating(PlayerTelemetryAggregate a) => Clamp(50 + Z(a.AvgPossessionTime, 120f, 30f) * 25 + Z(a.PossessionEfficiency, 0.7f, 0.15f) * 20 + Z(-a.TurnoverRate, 3.5f, 1.5f) * 25 + Z(a.PuckTouchesPerGame, 45f, 15f) * 15 + Z(a.StickActivity, 60f, 20f) * 15);
    public int ComputePassingRating(PlayerTelemetryAggregate a) => Clamp(50 + Z(a.PassCompletionRate, 0.72f, 0.12f) * 30 + Z(a.PassesPerGame, 25f, 8f) * 15 + Z(a.DangerousPassesPerGame, 2.5f, 1.2f) * 25 + Z(a.AssistsPerGame, 0.6f, 0.4f) * 20 + Z(a.PassDirectionDiversity, 0.6f, 0.15f) * 10);
    public int ComputeInputRating(PlayerTelemetryAggregate a) => Clamp(50 + Z(a.InputConsistency, 0.7f, 0.15f) * 25 + Z(a.InputComplexity, 0.55f, 0.2f) * 20 + Z(-a.AvgReactionMs, 250f, 60f) * 20 + Z(a.ActionsPerMinute, 80f, 25f) * 15 + Z(a.StickInputEfficiency, 0.6f, 0.15f) * 10 + Z(a.WasdBalance, 0.7f, 0.15f) * 10);
    public int ComputeStickMotionRating(PlayerTelemetryAggregate a) => Clamp(50 + Z(a.AvgStickSpeed, 120f, 30f) * 20 + Z(a.StickAnglePrecision, 0.65f, 0.15f) * 20 + Z(a.StickEfficiency, 0.55f, 0.15f) * 30 + Z(a.StickActivity, 60f, 20f) * 15 + Z(a.StickAngleRange, 0.6f, 0.2f) * 15);
    public int ComputeOffensiveRating(PlayerTelemetryAggregate a) => Clamp(50 + Z(a.OffensiveZoneTime, 0.3f, 0.1f) * 25 + Z(a.ShotCreationRate, 0.25f, 0.1f) * 25 + Z(a.ChanceCreationRate, 0.12f, 0.06f) * 25 + Z(a.OffensivePositioning, 0.4f, 0.1f) * 15 + Z(a.GoalsPerGame, 0.8f, 0.5f) * 10);
    public int ComputeDefensiveRating(PlayerTelemetryAggregate a) => Clamp(50 + Z(a.InterceptionsPerGame, 4f, 2f) * 25 + Z(a.BlocksPerGame, 2f, 1.2f) * 20 + Z(a.DefensivePositioning, 0.45f, 0.1f) * 20 + Z(a.PuckRecoveriesPerGame, 5f, 2.5f) * 15 + Z(a.ZoneCoverage, 0.6f, 0.1f) * 10 + Z(a.FaceoffWinRate, 0.48f, 0.1f) * 10);
    public int ComputePositioningRating(PlayerTelemetryAggregate a) => Clamp(50 + Z(a.ZoneOccupancyConsistency, 0.55f, 0.15f) * 25 + Z(a.AvgSpacingToTeammates, 0.4f, 0.1f) * 20 + Z(-a.PositionVariance, 2.5f, 1.2f) * 25 + Z(a.TeamSupportTime, 0.5f, 0.1f) * 20 + Z(a.TransitionSpeed, 0.6f, 0.15f) * 10);
    public int ComputeGameSenseRating(PlayerTelemetryAggregate a) => Clamp(50 + Z(a.WinRateAboveTeam, 0.02f, 0.08f) * 25 + Z(a.DecisionQuality, 0.55f, 0.15f) * 25 + Z(a.AnticipationScore, 0.5f, 0.15f) * 20 + Z(a.TeamSynergy, 0.45f, 0.1f) * 15 + Z(a.ClutchScore, 0.5f, 0.2f) * 15);

    public Percentiles ComputePercentiles(PlayerRatings r) => new()
    {
        SkatingPercentile = ToPct(r.Skating), ShootingPercentile = ToPct(r.Shooting),
        StickhandlingPercentile = ToPct(r.Stickhandling), PassingPercentile = ToPct(r.Passing),
        InputsPercentile = ToPct(r.Inputs), StickMotionPercentile = ToPct(r.StickMotion),
        OffensivePlayPercentile = ToPct(r.OffensivePlay), DefensivePlayPercentile = ToPct(r.DefensivePlay),
        PositioningPercentile = ToPct(r.Positioning), GameSensePercentile = ToPct(r.GameSense),
        OverallPercentile = ToPct(r.Overall),
    };

    public static int ToPct(int rating) => Math.Clamp((int)Math.Round(NormalCdf((rating - 50.0) / 15.0) * 100), 0, 99);

    public ScoutingReport GenerateScoutingReport(PlayerRatings r, PlayerTelemetryAggregate a)
    {
        var report = new ScoutingReport { OverallRating = r.Overall, Archetype = r.Archetype };
        if (r.Skating >= 75) report.Strengths.Add("Elite skating ability.");
        if (r.Shooting >= 75) report.Strengths.Add("Strong shooting accuracy.");
        if (r.Stickhandling >= 75) report.Strengths.Add("Excellent puck control.");
        if (r.Passing >= 75) report.Strengths.Add("Precise passing.");
        if (r.OffensivePlay >= 75) report.Strengths.Add("Strong offensive pressure.");
        if (r.DefensivePlay >= 75) report.Strengths.Add("Solid defensive coverage.");
        if (r.Positioning >= 75) report.Strengths.Add("Excellent positional awareness.");
        if (r.GameSense >= 75) report.Strengths.Add("High hockey IQ.");
        if (r.Shooting <= 35) report.Weaknesses.Add("Below-average shooting.");
        if (r.Skating <= 35) report.Weaknesses.Add("Below-average skating.");
        if (r.Stickhandling <= 35) report.Weaknesses.Add("Poor puck control.");
        if (r.DefensivePlay <= 35) report.Weaknesses.Add("Weak defensive coverage.");
        if (r.Positioning <= 35) report.Weaknesses.Add("Poor positional discipline.");
        if (report.Strengths.Count == 0) report.Strengths.Add("Balanced player with developing skills.");
        if (report.Weaknesses.Count == 0) report.Weaknesses.Add("No significant weaknesses identified.");
        if (r.Shooting < 40) report.ImprovementSuggestions.Add("Increase shot volume from high-danger areas.");
        if (r.DefensivePlay < 40) report.ImprovementSuggestions.Add("Improve defensive positioning.");
        if (r.Stickhandling < 40) report.ImprovementSuggestions.Add("Reduce neutral-zone turnovers.");
        report.Summary = r.Overall >= 90 ? "Elite player." : r.Overall >= 80 ? "Strong player." : r.Overall >= 65 ? "Solid player." : r.Overall >= 50 ? "Average player." : "Developing player.";
        return report;
    }

    private static double Z(double value, double mean, double std) => Math.Abs(std) < 0.001 ? 0 : (value - mean) / std;
    private static int Clamp(double v) => Math.Clamp((int)Math.Round(v), 0, 100);
    private static double NormalCdf(double x) { int sign = x < 0 ? -1 : 1; x = Math.Abs(x) / Math.Sqrt(2.0); double t = 1.0 / (1.0 + 0.3275911 * x); return 0.5 * (1.0 + sign * (1.0 - (((((1.061405429 * t - 1.453152027) * t + 1.421413741) * t - 0.284496736) * t + 0.254829592) * t * Math.Exp(-x * x)))); }
}
