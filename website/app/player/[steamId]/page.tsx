'use client';
import { useParams } from 'next/navigation';
import { useEffect, useState } from 'react';
import { api, type PlayerProfile } from '@/lib/api';
import { RadarChart } from '@/components/RadarChart';

export default function PlayerPage() {
  const params = useParams();
  const steamId = params.steamId as string;
  const [p, setP] = useState<PlayerProfile | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!steamId) return;
    setLoading(true);
    api.getPlayer(steamId).then(setP).catch(() => {}).finally(() => setLoading(false));
  }, [steamId]);

  if (loading) return <div className="flex items-center justify-center min-h-[60vh]"><div className="text-[11px] text-[#555]">Loading...</div></div>;
  if (!p) return <div className="flex items-center justify-center min-h-[60vh]"><div className="text-[11px] text-[#8a8a8a]">Player not found</div></div>;

  const r = p.ratings;
  const pc = p.percentiles;
  const gp = p.totalMatches;
  const toiH = p.totalTimeOnIce > 0 ? (p.totalTimeOnIce / 3600).toFixed(1) : '0';
  const shPct = p.totalShots > 0 ? ((p.totalGoals / p.totalShots) * 100).toFixed(1) : '0';
  const foPct = p.faceoffsTaken > 0 ? ((p.faceoffsWon / p.faceoffsTaken) * 100).toFixed(1) : '0';
  const ppPts = p.powerplayGoals + p.powerplayAssists;
  const shPts = p.shorthandedGoals + p.shorthandedAssists;

  return (
    <div className="max-w-[1200px] mx-auto px-2 py-2">
      {/* TOP: Player identity bar — dense, all info visible */}
      <div className="flex items-start gap-4 pb-3 mb-3 border-b border-[#1d1d1d]">
        {/* Avatar + name */}
        <div className="flex items-center gap-2 min-w-0">
          <div className="w-10 h-10 rounded-full bg-[#0b0b0b] border border-[#1d1d1d] flex items-center justify-center text-xs text-[#555] flex-shrink-0">
            {p.username.charAt(0).toUpperCase()}
          </div>
          <div>
            <div className="flex items-center gap-1.5">
              <h1 className="text-base font-bold text-[#e8e8e8]">{p.username}</h1>
              <span className="px-1 py-0.5 rounded bg-[#4ade80]/10 border border-[#4ade80]/20 text-[9px] text-[#4ade80]">{p.archetype}</span>
            </div>
            <div className="text-[9px] text-[#555] font-mono">{steamId}</div>
          </div>
        </div>

        {/* Compact stat clusters */}
        <div className="flex items-center gap-5 text-[11px] flex-wrap">
          <div className="text-center">
            <div className="text-lg font-bold text-[#e8e8e8]">{gp}</div>
            <div className="text-[9px] text-[#555] uppercase tracking-wider">GP</div>
          </div>
          <div className="text-center">
            <div className="text-lg font-bold text-[#e8e8e8]">{p.totalGoals}</div>
            <div className="text-[9px] text-[#555] uppercase tracking-wider">G</div>
          </div>
          <div className="text-center">
            <div className="text-lg font-bold text-[#e8e8e8]">{p.totalAssists}</div>
            <div className="text-[9px] text-[#555] uppercase tracking-wider">A</div>
          </div>
          <div className="text-center">
            <div className="text-lg font-bold text-[#e8e8e8]">{p.totalPoints}</div>
            <div className="text-[9px] text-[#555] uppercase tracking-wider">PTS</div>
          </div>
          <div className="text-center">
            <div className={`text-lg font-bold ${p.plusMinus > 0 ? 'text-[#4ade80]' : p.plusMinus < 0 ? 'text-[#f87171]' : 'text-[#e8e8e8]'}`}>{p.plusMinus > 0 ? '+' : ''}{p.plusMinus}</div>
            <div className="text-[9px] text-[#555] uppercase tracking-wider">+/-</div>
          </div>
          <div className="border-l border-[#1d1d1d] pl-3">
            <div className="flex items-center gap-2 text-[10px] text-[#555]">
              <span>PIM: {p.totalPim}</span>
              <span>TOI: {toiH}h</span>
              <span>SH%: {shPct}%</span>
              <span>FO%: {foPct}%</span>
            </div>
            <div className="text-[9px] text-[#555] mt-0.5">PP: {ppPts} | SH: {shPts} | HIT: {p.totalHits}</div>
          </div>
        </div>

        {/* OVR badge */}
        <div className="ml-auto flex-shrink-0 text-center">
          <div className="w-11 h-11 rounded-full border-2 border-[#4ade80]/50 flex items-center justify-center text-base font-bold text-[#4ade80]">{r.overall}</div>
          <div className="text-[8px] text-[#555] mt-0.5">OVR</div>
        </div>
      </div>

      {/* Two-column: left = radar + ratings, right = advanced stats */}
      <div className="grid grid-cols-[200px_1fr] gap-3">
        {/* LEFT COLUMN */}
        <div className="space-y-2">
          <div className="panel rounded p-1.5"><RadarChart data={[
            { category: 'SKT', value: r.skating, fullMark: 100 },
            { category: 'SHT', value: r.shooting, fullMark: 100 },
            { category: 'STK', value: r.stickhandling, fullMark: 100 },
            { category: 'PAS', value: r.passing, fullMark: 100 },
            { category: 'INP', value: r.inputs, fullMark: 100 },
            { category: 'GSN', value: r.gameSense, fullMark: 100 },
          ]} size={170} /></div>

          <div className="panel rounded">
            <div className="panel-header">Ratings</div>
            <div className="text-[10px] font-mono leading-loose p-2 text-[#8a8a8a]">
              {[
                ['Skating', r.skating],
                ['Shooting', r.shooting],
                ['Stickhandling', r.stickhandling],
                ['Passing', r.passing],
                ['Inputs', r.inputs],
                ['Game Sense', r.gameSense],
              ].map(([l, v]) => (
                <div key={l as string} className="flex items-center justify-between">
                  <span>{l as string}</span>
                  <span className="tracking-widest text-[#555]">. . . . . . . . .</span>
                  <span className={(v as number) > 60 ? 'text-[#4ade80]' : (v as number) > 35 ? 'text-[#fbbf24]' : 'text-[#f87171]'}>{v as number}</span>
                </div>
              ))}
            </div>
          </div>

          <div className="panel rounded">
            <div className="panel-header">Percentile</div>
            <div className="p-2 space-y-1">
              {[
                ['OVR', pc.overallPercentile],
                ['SKT', pc.skatingPercentile],
                ['SHT', pc.shootingPercentile],
                ['STK', pc.stickhandlingPercentile],
                ['PAS', pc.passingPercentile],
                ['GSN', pc.gameSensePercentile],
              ].map(([l, v]) => (
                <div key={l as string} className="flex items-center gap-1.5">
                  <span className="text-[9px] text-[#555] w-6">{l as string}</span>
                  <div className="flex-1 histogram-bar"><div className="histogram-fill bg-[#4ade80]/40" style={{ width: `${v || 50}%` }} /></div>
                  <span className="text-[9px] font-mono text-[#4ade80] w-6 text-right">{v || 50}</span>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* RIGHT COLUMN — traditional stats FIRST, then advanced */}
        <div className="space-y-2">
          {/* === TRADITIONAL STATS TABLE === */}
          <div className="panel rounded overflow-hidden">
            <table className="w-full text-[11px]">
              <thead>
                <tr className="border-b border-[#1d1d1d]">
                  <th className="table-header">GP</th><th className="table-header">G</th><th className="table-header">A</th><th className="table-header">PTS</th>
                  <th className="table-header">+/-</th><th className="table-header">PIM</th><th className="table-header">TOI</th><th className="table-header">SH%</th>
                  <th className="table-header">FO%</th><th className="table-header">HIT</th><th className="table-header">BLK</th><th className="table-header">INT</th>
                </tr>
              </thead>
              <tbody>
                <tr className="border-b border-[#1d1d1d]/50">
                  <td className="table-cell font-bold">{gp}</td>
                  <td className="table-cell">{p.totalGoals}</td>
                  <td className="table-cell">{p.totalAssists}</td>
                  <td className="table-cell font-bold">{p.totalPoints}</td>
                  <td className={`table-cell font-bold ${p.plusMinus > 0 ? 'text-[#4ade80]' : p.plusMinus < 0 ? 'text-[#f87171]' : ''}`}>{p.plusMinus > 0 ? '+' : ''}{p.plusMinus}</td>
                  <td className="table-cell">{p.totalPim}</td>
                  <td className="table-cell">{toiH}h</td>
                  <td className="table-cell">{shPct}%</td>
                  <td className="table-cell">{foPct}%</td>
                  <td className="table-cell">{p.totalHits}</td>
                  <td className="table-cell">{p.totalBlocks}</td>
                  <td className="table-cell">{p.totalInterceptions}</td>
                </tr>
              </tbody>
            </table>
          </div>

          {/* Shooting + Passing detail */}
          <div className="grid grid-cols-2 gap-2">
            <div className="panel rounded">
              <div className="panel-header">Shooting</div>
              <div className="grid grid-cols-2 gap-0">
                <div className="stat-row"><span className="stat-label">Goals</span><span className="stat-value">{p.totalGoals}</span></div>
                <div className="stat-row"><span className="stat-label">Shots</span><span className="stat-value">{p.totalShots}</span></div>
                <div className="stat-row"><span className="stat-label">SH%</span><span className="stat-value">{shPct}%</span></div>
                <div className="stat-row"><span className="stat-label">HD Goals</span><span className="stat-value">{p.highDangerGoals}</span></div>
                <div className="stat-row"><span className="stat-label">PPG</span><span className="stat-value">{p.powerplayGoals}</span></div>
                <div className="stat-row"><span className="stat-label">SHG</span><span className="stat-value">{p.shorthandedGoals}</span></div>
                <div className="stat-row"><span className="stat-label">GWG</span><span className="stat-value">{p.gameWinningGoals}</span></div>
              </div>
            </div>
            <div className="panel rounded">
              <div className="panel-header">Passing</div>
              <div className="grid grid-cols-2 gap-0">
                <div className="stat-row"><span className="stat-label">Assists</span><span className="stat-value">{p.totalAssists}</span></div>
                <div className="stat-row"><span className="stat-label">Primary</span><span className="stat-value">{p.primaryAssists}</span></div>
                <div className="stat-row"><span className="stat-label">Secondary</span><span className="stat-value">{p.secondaryAssists}</span></div>
                <div className="stat-row"><span className="stat-label">Passes</span><span className="stat-value">{p.totalPasses}</span></div>
                <div className="stat-row"><span className="stat-label">Completed</span><span className="stat-value">{p.passesCompleted}</span></div>
                <div className="stat-row"><span className="stat-label">Pass%</span><span className="stat-value">{p.totalPasses > 0 ? ((p.passesCompleted / p.totalPasses) * 100).toFixed(1) : '0'}%</span></div>
                <div className="stat-row"><span className="stat-label">Dangerous</span><span className="stat-value">{p.dangerousPasses}</span></div>
              </div>
            </div>
          </div>

          {/* Defense + Possession */}
          <div className="grid grid-cols-2 gap-2">
            <div className="panel rounded">
              <div className="panel-header">Defense</div>
              <div className="grid grid-cols-2 gap-0">
                <div className="stat-row"><span className="stat-label">Blocks</span><span className="stat-value">{p.totalBlocks}</span></div>
                <div className="stat-row"><span className="stat-label">Interceptions</span><span className="stat-value">{p.totalInterceptions}</span></div>
                <div className="stat-row"><span className="stat-label">Takeaways</span><span className="stat-value">{p.takeaways}</span></div>
                <div className="stat-row"><span className="stat-label">Stick Checks</span><span className="stat-value">{p.stickChecks}</span></div>
                <div className="stat-row"><span className="stat-label">Poke Checks</span><span className="stat-value">{p.pokeChecks}</span></div>
              </div>
            </div>
            <div className="panel rounded">
              <div className="panel-header">Possession</div>
              <div className="grid grid-cols-2 gap-0">
                <div className="stat-row"><span className="stat-label">Puck Touches</span><span className="stat-value">{p.totalPuckTouches}</span></div>
                <div className="stat-row"><span className="stat-label">Poss. Time</span><span className="stat-value">{(p.totalPossessionTime / 60).toFixed(1)}m</span></div>
                <div className="stat-row"><span className="stat-label">Turnovers</span><span className="stat-value">{p.turnovers}</span></div>
                <div className="stat-row"><span className="stat-label">Recoveries</span><span className="stat-value">{p.recoveries}</span></div>
                <div className="stat-row"><span className="stat-label">Faceoff%</span><span className="stat-value">{foPct}%</span></div>
              </div>
            </div>
          </div>

          {/* On-ice + Faceoffs */}
          <div className="grid grid-cols-2 gap-2">
            <div className="panel rounded">
              <div className="panel-header">On-Ice</div>
              <div className="grid grid-cols-3 gap-0">
                <div className="stat-row"><span className="stat-label">GF</span><span className="stat-value">{p.onIceGoalsFor}</span></div>
                <div className="stat-row"><span className="stat-label">GA</span><span className="stat-value">{p.onIceGoalsAgainst}</span></div>
                <div className="stat-row"><span className="stat-label">Diff</span><span className={`stat-value ${(p.onIceGoalsFor - p.onIceGoalsAgainst) > 0 ? 'text-[#4ade80]' : (p.onIceGoalsFor - p.onIceGoalsAgainst) < 0 ? 'text-[#f87171]' : ''}`}>{p.onIceGoalsFor - p.onIceGoalsAgainst}</span></div>
              </div>
            </div>
            <div className="panel rounded">
              <div className="panel-header">Faceoffs</div>
              <div className="grid grid-cols-3 gap-0">
                <div className="stat-row"><span className="stat-label">Taken</span><span className="stat-value">{p.faceoffsTaken}</span></div>
                <div className="stat-row"><span className="stat-label">Won</span><span className="stat-value">{p.faceoffsWon}</span></div>
                <div className="stat-row"><span className="stat-label">FO%</span><span className="stat-value">{foPct}%</span></div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
