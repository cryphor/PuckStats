'use client';

import { useParams } from 'next/navigation';
import { useEffect, useState } from 'react';
import { api, type PlayerProfile } from '@/lib/api';
import { RadarChart } from '@/components/RadarChart';
import { RatingBadge } from '@/components/RatingBadge';
import { RatingTrendChart } from '@/components/RatingTrendChart';
import { Loader2, TrendingUp, TrendingDown, Target, Shield, Zap } from 'lucide-react';

export default function PlayerPage() {
  const params = useParams();
  const steamId = params.steamId as string;
  const [profile, setProfile] = useState<PlayerProfile | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!steamId) return;
    setLoading(true);
    api.getPlayer(steamId)
      .then(setProfile)
      .catch((e) => setError(e.message))
      .finally(() => setLoading(false));
  }, [steamId]);

  if (loading) return <PageLoader />;
  if (error) return <ErrorDisplay message={error} />;
  if (!profile) return <ErrorDisplay message="Player not found" />;

  const radarData = [
    { category: 'Skating', value: profile.ratings.skating, fullMark: 100 },
    { category: 'Shooting', value: profile.ratings.shooting, fullMark: 100 },
    { category: 'Stickhandling', value: profile.ratings.stickhandling, fullMark: 100 },
    { category: 'Passing', value: profile.ratings.passing, fullMark: 100 },
    { category: 'Inputs', value: profile.ratings.inputs, fullMark: 100 },
    { category: 'Game Sense', value: profile.ratings.gameSense, fullMark: 100 },
  ];

  return (
    <div className="max-w-7xl mx-auto px-4 py-8 animate-fade-in">
      {/* Header */}
      <div className="glass-panel p-6 mb-6">
        <div className="flex items-start justify-between flex-wrap gap-4">
          <div className="flex items-center gap-4">
            <div className="w-16 h-16 rounded-full bg-panel border-2 border-accent/30 flex items-center justify-center">
              {profile.avatarUrl ? (
                <img src={profile.avatarUrl} alt="" className="w-full h-full rounded-full" />
              ) : (
                <span className="text-2xl font-bold text-accent">
                  {profile.username.charAt(0).toUpperCase()}
                </span>
              )}
            </div>
            <div>
              <h1 className="text-2xl font-bold text-text">{profile.username}</h1>
              <p className="text-sm text-muted">Steam ID: {profile.steamId}</p>
              <div className="flex items-center gap-3 mt-2">
                <span className="px-2 py-0.5 rounded-md bg-accent/10 border border-accent/20 text-accent text-xs font-medium">
                  {profile.archetype}
                </span>
                <span className="text-xs text-muted">
                  {profile.totalMatches} matches · {profile.totalGoals} goals · {profile.totalAssists} assists
                </span>
              </div>
            </div>
          </div>
          <div className="flex items-center gap-3">
            <div className="text-center">
              <div className="text-4xl font-extrabold text-accent text-glow">{profile.ratings.overall}</div>
              <div className="text-xs text-muted">OVR Rating</div>
            </div>
            <div className="text-center px-3">
              <div className="text-sm font-semibold text-text">
                Top {profile.percentiles.overallPercentile}%
              </div>
              <div className="text-xs text-muted">Percentile</div>
            </div>
          </div>
        </div>
      </div>

      <div className="grid lg:grid-cols-3 gap-6">
        {/* Left column - Radar + Ratings */}
        <div className="lg:col-span-1 space-y-4">
          <div className="glass-panel p-4 flex justify-center">
            <RadarChart data={radarData} size={260} />
          </div>

          <div className="glass-panel p-4">
            <h3 className="text-sm font-semibold text-text mb-3">Category Ratings</h3>
            <div className="space-y-2">
              {Object.entries(profile.ratings)
                .filter(([k]) => !['archetype', 'stickMotion'].includes(k))
                .map(([key, value]) => (
                  <RatingBar key={key} label={formatLabel(key)} value={value as number} />
                ))}
            </div>
          </div>
        </div>

        {/* Right column - Details */}
        <div className="lg:col-span-2 space-y-6">
          {/* Quick stats */}
          <div className="stat-grid">
            <QuickStat icon={<Zap size={16} />} label="Win Rate" value={`${(profile.winRate * 100).toFixed(0)}%`} />
            <QuickStat icon={<Target size={16} />} label="Goals/Game" value={profile.totalMatches > 0 ? (profile.totalGoals / profile.totalMatches).toFixed(1) : '0'} />
            <QuickStat icon={<TrendingUp size={16} />} label="Assists/Game" value={profile.totalMatches > 0 ? (profile.totalAssists / profile.totalMatches).toFixed(1) : '0'} />
            <QuickStat icon={<Shield size={16} />} label="Saves" value={profile.totalSaves.toString()} />
          </div>

          {/* Scouting Report */}
          <div className="glass-panel p-5">
            <h3 className="text-sm font-semibold text-text mb-3 flex items-center gap-2">
              <Zap size={14} className="text-accent" />
              AI Scouting Report
            </h3>
            <p className="text-sm text-muted mb-4">{profile.scoutingReport.summary}</p>
            <div className="grid md:grid-cols-2 gap-4">
              <div>
                <h4 className="text-xs font-semibold text-accent mb-2 uppercase tracking-wider">Strengths</h4>
                <ul className="space-y-1.5">
                  {profile.scoutingReport.strengths.map((s, i) => (
                    <li key={i} className="text-xs text-text flex items-start gap-2">
                      <span className="text-accent mt-0.5">+</span> {s}
                    </li>
                  ))}
                </ul>
              </div>
              <div>
                <h4 className="text-xs font-semibold text-warning mb-2 uppercase tracking-wider">Areas to Improve</h4>
                <ul className="space-y-1.5">
                  {profile.scoutingReport.weaknesses.map((w, i) => (
                    <li key={i} className="text-xs text-muted flex items-start gap-2">
                      <span className="text-warning mt-0.5">-</span> {w}
                    </li>
                  ))}
                </ul>
                {profile.scoutingReport.improvementSuggestions.length > 0 && (
                  <div className="mt-3 pt-3 border-t border-border">
                    <h4 className="text-xs font-semibold text-blue mb-2 uppercase tracking-wider">Suggestions</h4>
                    {profile.scoutingReport.improvementSuggestions.map((s, i) => (
                      <p key={i} className="text-xs text-muted mb-1">• {s}</p>
                    ))}
                  </div>
                )}
              </div>
            </div>
          </div>

          {/* Recent Matches */}
          <div className="glass-panel p-5">
            <h3 className="text-sm font-semibold text-text mb-3">Recent Matches</h3>
            {profile.recentMatches.length === 0 ? (
              <p className="text-xs text-muted">No matches recorded yet.</p>
            ) : (
              <div className="space-y-2">
                {profile.recentMatches.map((match, i) => (
                  <div key={i} className="flex items-center justify-between py-2 border-b border-border last:border-0">
                    <div className="flex items-center gap-3">
                      <div className={`w-2 h-2 rounded-full ${
                        (match.team === 'Blue' && match.teamScore > match.opponentScore) ||
                        (match.team === 'Red' && match.teamScore < match.opponentScore)
                          ? 'bg-accent' : 'bg-danger'
                      }`} />
                      <div>
                        <p className="text-xs font-medium text-text">{match.type} Match</p>
                        <p className="text-[10px] text-muted-dim">{match.date}</p>
                      </div>
                    </div>
                    <div className="flex items-center gap-4">
                      <span className="text-sm font-mono text-text">
                        {match.teamScore} - {match.opponentScore}
                      </span>
                      <span className="text-xs text-muted">
                        {match.goals}G / {match.assists}A
                      </span>
                      <RatingBadge rating={Math.round(match.rating)} label="" size="sm" />
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

function RatingBar({ label, value }: { label: string; value: number }) {
  return (
    <div className="flex items-center gap-3">
      <span className="text-xs text-muted w-24">{label}</span>
      <div className="flex-1 progress-bar">
        <div
          className="fill"
          style={{
            width: `${value}%`,
            background: `linear-gradient(90deg, #42ff8f ${value}%, #333 ${value}%)`,
          }}
        />
      </div>
      <span className="text-xs font-mono text-text w-8 text-right">{value}</span>
    </div>
  );
}

function QuickStat({ icon, label, value }: { icon: React.ReactNode; label: string; value: string }) {
  return (
    <div className="glass-panel p-3">
      <div className="flex items-center gap-2 text-muted">
        {icon}
        <span className="text-xs">{label}</span>
      </div>
      <p className="text-lg font-bold text-text mt-1">{value}</p>
    </div>
  );
}

function PageLoader() {
  return (
    <div className="flex items-center justify-center min-h-[60vh]">
      <Loader2 size={32} className="text-accent animate-spin" />
    </div>
  );
}

function ErrorDisplay({ message }: { message: string }) {
  return (
    <div className="flex items-center justify-center min-h-[60vh]">
      <div className="glass-panel p-8 text-center">
        <p className="text-danger text-sm mb-2">Error</p>
        <p className="text-muted text-sm">{message}</p>
      </div>
    </div>
  );
}

function formatLabel(key: string): string {
  return key.replace(/([A-Z])/g, ' $1').replace(/^./, (c) => c.toUpperCase()).trim();
}
