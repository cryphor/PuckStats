import { NextRequest, NextResponse } from 'next/server';
import { getPlayer, initTables } from '@/lib/db';

export async function GET(req: NextRequest, { params }: { params: { steamId: string } }) {
  try {
    await initTables();
    const steamId = params.steamId;
    const p = await getPlayer(steamId);
    const s = (v: any) => v ?? 0;
    const sid = steamId;

    if (!p) {
      return NextResponse.json({
        steamId: sid, username: sid.substring(0, 12), avatarUrl: '',
        archetype: 'Unknown',
        totalMatches: 0, totalWins: 0, totalLosses: 0, totalOtl: 0,
        totalGoals: 0, totalAssists: 0, totalPoints: 0, plusMinus: 0, totalPim: 0, totalTimeOnIce: 0,
        totalShots: 0, totalHits: 0, totalDistanceTraveled: 0, totalPlayTimeSeconds: 0,
        totalPuckTouches: 0, totalPossessionTime: 0, averageSpeed: 0, topSpeed: 0,
        turnovers: 0, recoveries: 0, powerplayGoals: 0, shorthandedGoals: 0, gameWinningGoals: 0,
        faceoffsTaken: 0, faceoffsWon: 0,
        onIceGoalsFor: 0, onIceGoalsAgainst: 0,
        totalSaves: 0, winRate: 0,
        ratings: { skating: 50, shooting: 50, stickhandling: 50, passing: 50, inputs: 50, stickMotion: 50, offensivePlay: 50, defensivePlay: 50, positioning: 50, gameSense: 50, overall: 50 },
        percentiles: { overallPercentile: 50 },
        scoutingReport: { strengths: [], weaknesses: [], overallRating: 50, summary: 'No data yet.' },
        recentMatches: [], lastUpdated: new Date().toISOString(),
      });
    }

    const uid = s(p.Username || p.username);
    const gp = s(p.TotalMatches || p.totalmatches);

    return NextResponse.json({
      steamId: sid,
      username: uid || sid.substring(0, 12),
      avatarUrl: '',
      archetype: s(p.Archetype || p.archetype) || 'Unknown',
      totalMatches: gp,
      totalWins: s(p.TotalWins || p.totalwins),
      totalLosses: s(p.TotalLosses || p.totallosses),
      totalGoals: s(p.TotalGoals || p.totalgoals),
      totalAssists: s(p.TotalAssists || p.totalassists),
      totalPoints: s(p.TotalPoints || p.totalpoints || p.TotalGoals || 0),
      plusMinus: s(p.PlusMinus || p.plusminus),
      totalPim: s(p.TotalPim || p.totalpim),
      totalTimeOnIce: s(p.TotalTimeOnIce || p.totaltimeonice),
      totalShots: s(p.TotalShots || p.totalshots),
      totalHits: s(p.TotalHits || p.totalhits),
      totalDistanceTraveled: s(p.TotalDistanceTraveled || p.totaldistancetraveled),
      totalPlayTimeSeconds: s(p.TotalPlayTimeSeconds || p.totalplaytimeseconds),
      totalPuckTouches: s(p.TotalPuckTouches || p.totalpucktouches),
      totalPossessionTime: s(p.TotalPossessionTime || p.totalpossessiontime),
      averageSpeed: s(p.AverageSpeed || p.averagespeed),
      topSpeed: s(p.TopSpeed || p.topspeed),
      turnovers: s(p.Turnovers || p.turnovers),
      recoveries: s(p.Recoveries || p.recoveries),
      powerplayGoals: s(p.PowerplayGoals || p.powerplaygoals),
      shorthandedGoals: s(p.ShorthandedGoals || p.shorthandedgoals),
      gameWinningGoals: s(p.GameWinningGoals || p.gamewinninggoals),
      faceoffsTaken: s(p.FaceoffsTaken || p.faceoffstaken),
      faceoffsWon: s(p.FaceoffsWon || p.faceoffswon),
      onIceGoalsFor: s(p.OnIceGoalsFor || p.onicegoalsfor),
      onIceGoalsAgainst: s(p.OnIceGoalsAgainst || p.onicegoalsagainst),
      highDangerGoals: 0, lowDangerGoals: 0,
      primaryAssists: 0, secondaryAssists: 0,
      totalPasses: 0, passesCompleted: 0, dangerousPasses: 0,
      totalBlocks: 0, totalInterceptions: 0, takeaways: 0, stickChecks: 0, pokeChecks: 0,
      powerplayAssists: 0, shorthandedAssists: 0,
      totalSaves: 0,
      winRate: gp > 0 ? s(p.TotalWins) / gp : 0,
      ratings: { skating: 50, shooting: 50, stickhandling: 50, passing: 50, inputs: 50, stickMotion: 50, offensivePlay: 50, defensivePlay: 50, positioning: 50, gameSense: 50, overall: 50 },
      percentiles: { overallPercentile: 50 },
      scoutingReport: { strengths: [], weaknesses: [], overallRating: 50, summary: gp > 0 ? 'Stats available.' : 'No data yet.' },
      recentMatches: [],
      lastUpdated: new Date().toISOString(),
    });
  } catch (error: any) {
    console.error('Player error:', error?.message);
    return NextResponse.json({ error: error?.message || 'Error' }, { status: 500 });
  }
}
