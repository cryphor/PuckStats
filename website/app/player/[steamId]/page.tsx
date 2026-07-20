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

  if (loading) return <div className="flex items-center justify-center min-h-[60vh]"><div className="text-xs text-[#555]">Loading...</div></div>;

  if (!p) return (
    <div className="max-w-[800px] mx-auto px-3 py-8 text-center">
      <div className="text-lg font-bold text-[#e8e8e8] mb-1">Player not found</div>
      <div className="text-xs text-[#555]">No data matches that Steam ID. Play a match with the mod to register.</div>
    </div>
  );

  const r = p.ratings;
  const pc = p.percentiles;
  const gp = p.totalMatches;
  const toiH = p.totalTimeOnIce > 0 ? (p.totalTimeOnIce / 3600).toFixed(1) : '0';
  const shPct = p.totalShots > 0 ? ((p.totalGoals / p.totalShots) * 100).toFixed(1) : '0';
  const foPct = p.faceoffsTaken > 0 ? ((p.faceoffsWon / p.faceoffsTaken) * 100).toFixed(1) : '0';
  const gpg = gp > 0 ? (p.totalGoals / gp).toFixed(2) : '0';
  const apg = gp > 0 ? (p.totalAssists / gp).toFixed(2) : '0';

  return (
    <div className="max-w-[1200px] mx-auto px-3 py-3">

      {/* ===== PLAYER HEADER ===== */}
      <div className="flex items-end gap-5 pb-4 mb-4 border-b border-[#1a1a1a]">
        <div className="flex items-center gap-3">
          <div className="w-12 h-12 rounded-full bg-[#0b0b0b] border border-[#1a1a1a] flex items-center justify-center text-lg font-bold text-[#555] flex-shrink-0">
            {p.username.charAt(0).toUpperCase()}
          </div>
          <div>
            <h1 className="text-3xl font-bold text-[#e8e8e8] tracking-tight">{p.username}</h1>
            <div className="text-[12px] text-[#9d9d9d] mt-0.5">
              Attacker · Right Handed · <span className="text-[#5dbf72]">{p.archetype}</span>
            </div>
          </div>
        </div>
        <div className="ml-auto flex items-center gap-4">
          <div className="text-right">
            <div className="text-2xl font-bold text-[#4a7cff]">{r.overall}</div>
            <div className="text-[10px] text-[#555]">OVR</div>
          </div>
          <div className="text-right">
            <div className="text-2xl font-bold text-[#e8e8e8]">{gp}</div>
            <div className="text-[10px] text-[#555]">GP</div>
          </div>
          <div className="text-right">
            <div className="text-2xl font-bold text-[#e8e8e8]">{p.totalGoals}</div>
            <div className="text-[10px] text-[#555]">G</div>
          </div>
          <div className="text-right">
            <div className="text-2xl font-bold text-[#e8e8e8]">{p.totalAssists}</div>
            <div className="text-[10px] text-[#555]">A</div>
          </div>
          <div className="text-right">
            <div className="text-2xl font-bold text-[#e8e8e8]">{p.totalPoints}</div>
            <div className="text-[10px] text-[#555]">PTS</div>
          </div>
          <div className="text-right">
            <div className={`text-2xl font-bold ${p.plusMinus > 0 ? 'text-[#5dbf72]' : p.plusMinus < 0 ? 'text-[#cf5c5c]' : 'text-[#e8e8e8]'}`}>{p.plusMinus > 0 ? '+' : ''}{p.plusMinus}</div>
            <div className="text-[10px] text-[#555]">+/-</div>
          </div>
        </div>
      </div>

      {/* ===== TWO-COLUMN LAYOUT ===== */}
      <div className="grid grid-cols-[1fr_190px] gap-4">
        {/* === LEFT: ALL STATS === */}
        <div className="space-y-4">

          {/* PRIMARY STATS ROW — large numbers, scannable */}
          <div>
            <div className="section-title">Season Totals</div>
            <table className="w-full text-center">
              <thead>
                <tr className="border-b border-[#1a1a1a]/30">
                  <th className="th text-center">GP</th><th className="th text-center">G</th><th className="th text-center">A</th>
                  <th className="th text-center">PTS</th><th className="th text-center">+/-</th><th className="th text-center">PIM</th>
                  <th className="th text-center">TOI</th><th className="th text-center">SH%</th><th className="th text-center">FO%</th>
                  <th className="th text-center">HIT</th><th className="th text-center">BLK</th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <td className="td text-center text-[15px] font-semibold">{gp}</td>
                  <td className="td text-center text-[15px] font-semibold">{p.totalGoals}</td>
                  <td className="td text-center text-[15px] font-semibold">{p.totalAssists}</td>
                  <td className="td text-center text-[15px] font-semibold">{p.totalPoints}</td>
                  <td className={`td text-center text-[15px] font-semibold ${p.plusMinus > 0 ? 'pos' : p.plusMinus < 0 ? 'neg' : ''}`}>{p.plusMinus > 0 ? '+' : ''}{p.plusMinus}</td>
                  <td className="td text-center">{p.totalPim}</td>
                  <td className="td text-center">{toiH}h</td>
                  <td className="td text-center">{shPct}%</td>
                  <td className="td text-center">{foPct}%</td>
                  <td className="td text-center">{p.totalHits}</td>
                  <td className="td text-center">{p.totalBlocks}</td>
                </tr>
              </tbody>
            </table>
          </div>

          {/* STAT GROUPS — 3 columns: Scoring | Passing | Possession */}
          <div className="grid grid-cols-3 gap-5">
            {/* Scoring */}
            <div>
              <div className="section-title">Scoring</div>
              <StatRow label="Goals" val={p.totalGoals} />
              <StatRow label="Assists" val={p.totalAssists} />
              <StatRow label="Points" val={p.totalPoints} />
              <StatRow label="G/GP" val={gpg} />
              <StatRow label="A/GP" val={apg} />
              <StatRow label="PPG" val={p.powerplayGoals} />
              <StatRow label="SHG" val={p.shorthandedGoals} />
              <StatRow label="GWG" val={p.gameWinningGoals} />
            </div>
            {/* Shooting */}
            <div>
              <div className="section-title">Shooting</div>
              <StatRow label="Shots" val={p.totalShots} />
              <StatRow label="SH%" val={shPct + '%'} />
              <StatRow label="Shot Att." val={p.totalShotAttempts} />
              <StatRow label="HD Goals" val={p.highDangerGoals} />
              <StatRow label="LD Goals" val={p.lowDangerGoals} />
              <div className="mt-2 section-title">Faceoffs</div>
              <StatRow label="FO Won" val={p.faceoffsWon} />
              <StatRow label="FO Lost" val={p.faceoffsTaken - p.faceoffsWon} />
              <StatRow label="FO%" val={foPct + '%'} />
            </div>
            {/* Possession */}
            <div>
              <div className="section-title">Possession</div>
              <StatRow label="Poss. Time" val={(p.totalPossessionTime / 60).toFixed(1) + 'm'} />
              <StatRow label="Puck Touches" val={p.totalPuckTouches} />
              <StatRow label="Turnovers" val={p.turnovers} />
              <StatRow label="Recoveries" val={p.recoveries} />
              <div className="mt-2 section-title">Passing</div>
              <StatRow label="Passes" val={p.totalPasses} />
              <StatRow label="Completed" val={p.passesCompleted} />
              <StatRow label="Pass%" val={p.totalPasses > 0 ? ((p.passesCompleted / p.totalPasses) * 100).toFixed(1) + '%' : '0%'} />
              <StatRow label="Dangerous" val={p.dangerousPasses} />
            </div>
          </div>

          {/* DEFENSE + ON-ICE + RECORD */}
          <div className="grid grid-cols-3 gap-5">
            <div>
              <div className="section-title">Defense</div>
              <StatRow label="Blocks" val={p.totalBlocks} />
              <StatRow label="Interceptions" val={p.totalInterceptions} />
              <StatRow label="Takeaways" val={p.takeaways} />
              <StatRow label="Stick Checks" val={p.stickChecks} />
              <StatRow label="Poke Checks" val={p.pokeChecks} />
              <StatRow label="Hits" val={p.totalHits} />
            </div>
            <div>
              <div className="section-title">On-Ice</div>
              <StatRow label="Goals For" val={p.onIceGoalsFor} />
              <StatRow label="Goals Against" val={p.onIceGoalsAgainst} />
              <StatRow label="Differential" val={p.onIceGoalsFor - p.onIceGoalsAgainst} valClass={(p.onIceGoalsFor - p.onIceGoalsAgainst) > 0 ? 'pos' : (p.onIceGoalsFor - p.onIceGoalsAgainst) < 0 ? 'neg' : ''} />
              <div className="mt-2 section-title">Record</div>
              <StatRow label="Wins" val={p.totalWins} />
              <StatRow label="Losses" val={p.totalLosses} />
              <StatRow label="OTL" val={p.totalOtl} />
              <StatRow label="Win%" val={gp > 0 ? ((p.totalWins / gp) * 100).toFixed(1) + '%' : '0%'} />
            </div>
            <div>
              <div className="section-title">Special Teams</div>
              <StatRow label="PP Goals" val={p.powerplayGoals} />
              <StatRow label="PP Assists" val={p.powerplayAssists} />
              <StatRow label="PP Points" val={p.powerplayGoals + p.powerplayAssists} />
              <StatRow label="SH Goals" val={p.shorthandedGoals} />
              <StatRow label="SH Assists" val={p.shorthandedAssists} />
              <StatRow label="SH Points" val={p.shorthandedGoals + p.shorthandedAssists} />
            </div>
          </div>

          {/* HISTOGRAMS */}
          <div>
            <div className="section-title">Performance Distribution</div>
            <div className="grid grid-cols-3 gap-4">
              {[
                { label: 'Speed', val: 72, pct: 85 },
                { label: 'Agility', val: 58, pct: 62 },
                { label: 'Shot Acc.', val: 41, pct: 35 },
                { label: 'Possession', val: 68, pct: 78 },
                { label: 'Passing', val: 55, pct: 58 },
                { label: 'Defense', val: 38, pct: 28 },
              ].map(h => (
                <div key={h.label}>
                  <div className="text-[11px] text-[#9d9d9d] mb-0.5">{h.label}</div>
                  <div className="h-bar">
                    <div className="h-fill bg-[#5dbf72]/30" style={{ width: `${h.pct}%` }} />
                    <div className="h-marker" style={{ left: `${h.val}%` }} />
                  </div>
                  <div className="flex justify-between text-[9px] text-[#555] mt-0.5">
                    <span>P{h.pct}</span>
                    <span className="text-[#9d9d9d]">{h.val}%</span>
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* MATCH HISTORY */}
          <div>
            <div className="section-title">Match History</div>
            {p.recentMatches.length === 0 ? (
              <div className="text-[12px] text-[#555] py-1">No matches recorded.</div>
            ) : (
              <table className="w-full">
                <thead>
                  <tr className="border-b border-[#1a1a1a]/30">
                    <th className="th">Date</th><th className="th">Type</th><th className="th">Result</th>
                    <th className="th text-center">G</th><th className="th text-center">A</th><th className="th text-center">PTS</th>
                    <th className="th text-center">+/-</th><th className="th text-right">Rating</th>
                  </tr>
                </thead>
                <tbody>
                  {p.recentMatches.slice(0, 10).map((m, i) => (
                    <tr key={i} className="hover:bg-[#ffffff]/[0.02]">
                      <td className="td text-[#555]">{new Date(m.date).toLocaleDateString()}</td>
                      <td className="td">{m.type}</td>
                      <td className="td">{m.teamScore}-{m.opponentScore}</td>
                      <td className="td text-center">{m.goals}</td>
                      <td className="td text-center">{m.assists}</td>
                      <td className="td text-center">{m.goals + m.assists}</td>
                      <td className="td text-center font-medium">{m.teamScore - m.opponentScore}</td>
                      <td className="td text-right font-mono">{Math.round(m.rating)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>

          {/* CAREER TOTALS */}
          <div>
            <div className="section-title">Career</div>
            <table className="w-full text-center">
              <thead>
                <tr className="border-b border-[#1a1a1a]/30">
                  <th className="th text-center">GP</th><th className="th text-center">G</th><th className="th text-center">A</th>
                  <th className="th text-center">PTS</th><th className="th text-center">+/-</th><th className="th text-center">PIM</th>
                  <th className="th text-center">SH%</th><th className="th text-center">FO%</th><th className="th text-center">TOI</th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <td className="td text-center font-semibold">{gp}</td>
                  <td className="td text-center">{p.totalGoals}</td>
                  <td className="td text-center">{p.totalAssists}</td>
                  <td className="td text-center font-semibold">{p.totalPoints}</td>
                  <td className={`td text-center ${p.plusMinus > 0 ? 'pos' : p.plusMinus < 0 ? 'neg' : ''}`}>{p.plusMinus > 0 ? '+' : ''}{p.plusMinus}</td>
                  <td className="td text-center">{p.totalPim}</td>
                  <td className="td text-center">{shPct}%</td>
                  <td className="td text-center">{foPct}%</td>
                  <td className="td text-center">{toiH}h</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        {/* === RIGHT SIDEBAR: Radar + Ratings + Percentiles === */}
        <div className="space-y-3">
          {/* Radar */}
          <div className="bg-[#0b0b0b] border border-[#1a1a1a] rounded p-2">
            <RadarChart data={[
              { category: 'SKT', value: r.skating, fullMark: 100 },
              { category: 'SHT', value: r.shooting, fullMark: 100 },
              { category: 'STK', value: r.stickhandling, fullMark: 100 },
              { category: 'PAS', value: r.passing, fullMark: 100 },
              { category: 'INP', value: r.inputs, fullMark: 100 },
              { category: 'GSN', value: r.gameSense, fullMark: 100 },
            ]} size={160} />
          </div>

          {/* Ratings */}
          <div className="bg-[#0b0b0b] border border-[#1a1a1a] rounded p-2">
            <div className="section-title">Advanced Ratings</div>
            {[
              ['Skating', r.skating],
              ['Shooting', r.shooting],
              ['Stickhandling', r.stickhandling],
              ['Passing', r.passing],
              ['Inputs', r.inputs],
              ['Stick Motion', r.stickMotion],
              ['Offensive', r.offensivePlay],
              ['Defensive', r.defensivePlay],
              ['Positioning', r.positioning],
              ['Game Sense', r.gameSense],
            ].map(([l, v]) => (
              <div key={l as string} className="flex items-center justify-between py-[1px]">
                <span className="text-[10px] text-[#9d9d9d]">{l as string}</span>
                <span className="tracking-[0.3em] text-[#333] text-[9px]">. . . . . . . . .</span>
                <span className={`text-[11px] font-medium tabular-nums ${(v as number) > 60 ? 'text-[#5dbf72]' : (v as number) > 35 ? 'text-[#d8a441]' : 'text-[#cf5c5c]'}`}>{v as number}</span>
              </div>
            ))}
          </div>

          {/* Percentiles */}
          <div className="bg-[#0b0b0b] border border-[#1a1a1a] rounded p-2">
            <div className="section-title">Percentiles</div>
            {[
              ['OVR', pc.overallPercentile],
              ['SKT', pc.skatingPercentile],
              ['SHT', pc.shootingPercentile],
              ['STK', pc.stickhandlingPercentile],
              ['PAS', pc.passingPercentile],
              ['GSN', pc.gameSensePercentile],
            ].map(([l, v]) => (
              <div key={l as string} className="flex items-center gap-1.5 py-[2px]">
                <span className="text-[9px] text-[#555] w-5">{l as string}</span>
                <div className="flex-1 bar"><div className="bar-fill bg-[#4a7cff]/50" style={{ width: `${v || 50}%` }} /></div>
                <span className="text-[10px] font-mono text-[#4a7cff] w-5 text-right">{v || 50}</span>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}

function StatRow({ label, val, valClass }: { label: string; val: string | number; valClass?: string }) {
  return (
    <div className="flex items-center justify-between py-[2px]">
      <span className="text-[11px] text-[#9d9d9d]">{label}</span>
      <span className={`text-[12px] font-medium tabular-nums text-[#e8e8e8] ${valClass || ''}`}>{val}</span>
    </div>
  );
}
