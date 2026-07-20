import { NextRequest, NextResponse } from 'next/server';
import { query, initTables } from '@/lib/db';

export async function GET(req: NextRequest, { params }: { params: { steamId: string } }) {
  try {
    await initTables();
    const { steamId } = params;
    const playerResult = await query('SELECT * FROM "Players" WHERE "SteamId" = $1', [steamId]);
    const p = playerResult.rows[0] as any;
    const s = (v: any) => v ?? 0;
    const sid = steamId;

    // Default profile for new players
    if (!p) {
      return NextResponse.json({
        steamId: sid, username: sid.substring(0, 12), avatarUrl: '',
        archetype: 'Unknown',
        totalMatches: 0, totalWins: 0, totalLosses: 0, totalOtl: 0,
        totalGoals: 0, totalAssists: 0, totalPoints: 0, plusMinus: 0, totalPim: 0, totalTimeOnIce: 0,
        totalShots: 0, totalShotAttempts: 0, totalHits: 0,
        highDangerGoals: 0, lowDangerGoals: 0, powerplayGoals: 0, shorthandedGoals: 0, gameWinningGoals: 0,
        primaryAssists: 0, secondaryAssists: 0, totalPasses: 0, passesCompleted: 0, dangerousPasses: 0,
        faceoffsTaken: 0, faceoffsWon: 0,
        totalPossessionTime: 0, totalPuckTouches: 0, turnovers: 0, recoveries: 0,
        totalBlocks: 0, totalInterceptions: 0, takeaways: 0, stickChecks: 0, pokeChecks: 0,
        powerplayAssists: 0, shorthandedAssists: 0,
        onIceGoalsFor: 0, onIceGoalsAgainst: 0,
        totalSaves: 0, winRate: 0,
        ratings: { skating: 50, shooting: 50, stickhandling: 50, passing: 50, inputs: 50, stickMotion: 50, offensivePlay: 50, defensivePlay: 50, positioning: 50, gameSense: 50, overall: 50 },
        percentiles: { overallPercentile: 50 },
        scoutingReport: { strengths: [], weaknesses: [], overallRating: 50, summary: 'No data yet.' },
        recentMatches: [], lastUpdated: new Date().toISOString(),
      });
    }

    const rResult = await query('SELECT * FROM "PlayerRatings" WHERE "SteamId" = $1', [steamId]);
    const r = rResult.rows[0] as any || {};

    const g = s(p.totalgoals ?? p.TotalGoals);
    const a = s(p.totalassists ?? p.TotalAssists);
    const gp = s(p.totalmatches ?? p.TotalMatches);
    const w = s(p.totalwins ?? p.TotalWins);

    return NextResponse.json({
      steamId: sid,
      username: p.username ?? p.Username ?? sid.substring(0, 12),
      avatarUrl: p.avatarurl ?? p.AvatarUrl ?? '',
      archetype: p.archetype ?? p.Archetype ?? 'Unknown',
      // Basic
      totalMatches: gp,
      totalWins: w,
      totalLosses: s(p.totallosses ?? p.TotalLosses),
      totalOtl: s(p.totalotl ?? p.TotalOtl),
      // Scoring
      totalGoals: g,
      totalAssists: a,
      totalPoints: g + a,
      plusMinus: s(p.plusminus ?? p.PlusMinus),
      totalPim: s(p.totalpim ?? p.TotalPim),
      totalTimeOnIce: s(p.totaltimeonice ?? p.TotalTimeOnIce),
      totalShots: s(p.totalshots ?? p.TotalShots),
      totalShotAttempts: s(p.totalshotattempts ?? p.TotalShotAttempts),
      totalHits: s(p.totalhits ?? p.TotalHits),
      // Shooting detail
      highDangerGoals: s(p.highdangergoals ?? p.HighDangerGoals),
      lowDangerGoals: s(p.lowdangergoals ?? p.LowDangerGoals),
      powerplayGoals: s(p.powerplaygoals ?? p.PowerplayGoals),
      shorthandedGoals: s(p.shorthandedgoals ?? p.ShorthandedGoals),
      gameWinningGoals: s(p.gamewinninggoals ?? p.GameWinningGoals),
      // Passing
      primaryAssists: s(p.primaryassists ?? p.PrimaryAssists),
      secondaryAssists: s(p.secondaryassists ?? p.SecondaryAssists),
      totalPasses: s(p.totalpasses ?? p.TotalPasses),
      passesCompleted: s(p.passescompleted ?? p.PassesCompleted),
      dangerousPasses: s(p.dangerouspasses ?? p.DangerousPasses),
      // Faceoffs
      faceoffsTaken: s(p.faceoffstaken ?? p.FaceoffsTaken),
      faceoffsWon: s(p.faceoffswon ?? p.FaceoffsWon),
      // Possession
      totalPossessionTime: s(p.totalpossessiontime ?? p.TotalPossessionTime),
      totalPuckTouches: s(p.totalpucktouches ?? p.TotalPuckTouches),
      turnovers: s(p.turnovers ?? p.Turnovers),
      recoveries: s(p.recoveries ?? p.Recoveries),
      // Defense
      totalBlocks: s(p.totalblocks ?? p.TotalBlocks),
      totalInterceptions: s(p.totalinterceptions ?? p.TotalInterceptions),
      takeaways: s(p.takeaways ?? p.Takeaways),
      stickChecks: s(p.stickchecks ?? p.StickChecks),
      pokeChecks: s(p.pokechecks ?? p.PokeChecks),
      // Special teams
      powerplayAssists: s(p.powerplayassists ?? p.PowerplayAssists),
      shorthandedAssists: s(p.shorthandedassists ?? p.ShorthandedAssists),
      // On-ice
      onIceGoalsFor: s(p.onicegoalsfor ?? p.OnIceGoalsFor),
      onIceGoalsAgainst: s(p.onicegoalsagainst ?? p.OnIceGoalsAgainst),
      // Legacy
      totalSaves: 0,
      winRate: gp > 0 ? w / gp : 0,
      ratings: {
        skating: s(r.skating ?? r.Skating),
        shooting: s(r.shooting ?? r.Shooting),
        stickhandling: s(r.stickhandling ?? r.Stickhandling),
        passing: s(r.passing ?? r.Passing),
        inputs: s(r.inputs ?? r.Inputs),
        stickMotion: s(r.stickmotion ?? r.StickMotion),
        offensivePlay: s(r.offensiveplay ?? r.OffensivePlay),
        defensivePlay: s(r.defensiveplay ?? r.DefensivePlay),
        positioning: s(r.positioning ?? r.Positioning),
        gameSense: s(r.gamesense ?? r.GameSense),
        overall: s(r.overall ?? r.Overall),
      },
      percentiles: { overallPercentile: s(r.overallpercentile ?? r.OverallPercentile) },
      scoutingReport: { strengths: [], weaknesses: [], overallRating: s(r.overall ?? r.Overall), summary: gp > 0 ? 'Stats available.' : 'No data yet.' },
      recentMatches: [],
      lastUpdated: new Date().toISOString(),
    });
  } catch (error: any) {
    console.error('Player error:', error?.message);
    return NextResponse.json({ error: error?.message || 'Error' }, { status: 500 });
  }
}
