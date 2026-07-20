import Link from 'next/link';
import { Activity, BarChart3, Trophy, Film, Sparkles, ArrowRight } from 'lucide-react';

export default function HomePage() {
  return (
    <div className="max-w-7xl mx-auto px-4 py-12">
      {/* Hero */}
      <div className="text-center mb-16">
        <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full border border-accent/20 bg-accent/5 text-accent text-xs font-medium mb-6">
          <Sparkles size={12} />
          Early Access — Free for all players
        </div>
        <h1 className="text-5xl md:text-7xl font-extrabold tracking-tight mb-4">
          <span className="text-text">Puck</span>
          <span className="text-accent text-glow">Stats</span>
        </h1>
        <p className="text-lg text-muted max-w-2xl mx-auto leading-relaxed">
          The definitive analytics platform for Puck. Track your performance, analyze replays,
          compare against the best, and elevate your game with AI-powered insights.
        </p>
        <div className="flex items-center justify-center gap-3 mt-8">
          <Link href="/analytics" className="btn btn-primary flex items-center gap-2">
            <BarChart3 size={16} />
            View Analytics
            <ArrowRight size={16} />
          </Link>
          <Link href="/replay" className="btn btn-secondary flex items-center gap-2">
            <Film size={16} />
            Upload Replay
          </Link>
        </div>
      </div>

      {/* Feature grid */}
      <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-4 mb-16">
        <FeatureCard
          icon={<Activity size={24} className="text-accent" />}
          title="Real-Time Telemetry"
          description="Track every movement, input, and action during matches. 20Hz telemetry with instant visualization."
        />
        <FeatureCard
          icon={<Film size={24} className="text-accent" />}
          title="Replay Analysis"
          description="Upload .puckreplay files for deep analysis. Heatmaps, route tracking, and passing networks."
        />
        <FeatureCard
          icon={<BarChart3 size={24} className="text-accent" />}
          title="Skill Ratings"
          description="Get 0-100 ratings in 10 categories. Find your archetype, strengths, and areas to improve."
        />
        <FeatureCard
          icon={<Trophy size={24} className="text-accent" />}
          title="Global Leaderboards"
          description="See how you rank against every player. Compare percentiles across all skill categories."
        />
        <FeatureCard
          icon={<Sparkles size={24} className="text-accent" />}
          title="AI Coaching"
          description="Automated scouting reports with personalized improvement suggestions based on your playstyle."
        />
        <FeatureCard
          icon={<Activity size={24} className="text-accent" />}
          title="Shift Analytics"
          description="Detailed breakdown of every shift: distance, speed, possession, and offensive/defensive impact."
        />
      </div>

      {/* Quick Start */}
      <div className="glass-panel p-8 text-center max-w-3xl mx-auto">
        <h2 className="text-2xl font-bold text-text mb-3">Ready to get started?</h2>
        <p className="text-muted mb-6">
          Install the PuckStats mod from the Steam Workshop to begin tracking your matches automatically.
        </p>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 text-left">
          <QuickStep number="1" title="Install the Mod" desc="Subscribe on Steam Workshop or drop the DLL into your Plugins folder." />
          <QuickStep number="2" title="Play a Match" desc="Data collects automatically. The overlay shows your real-time stats." />
          <QuickStep number="3" title="Analyze" desc="Review your ratings, heatmaps, and scouting reports on your dashboard." />
        </div>
      </div>
    </div>
  );
}

function FeatureCard({ icon, title, description }: { icon: React.ReactNode; title: string; description: string }) {
  return (
    <div className="glass-panel-hover p-5 group cursor-default">
      <div className="mb-3 w-10 h-10 rounded-lg bg-accent/10 flex items-center justify-center group-hover:bg-accent/15 transition-colors">
        {icon}
      </div>
      <h3 className="text-sm font-semibold text-text mb-1.5">{title}</h3>
      <p className="text-xs text-muted leading-relaxed">{description}</p>
    </div>
  );
}

function QuickStep({ number, title, desc }: { number: string; title: string; desc: string }) {
  return (
    <div className="flex gap-3">
      <div className="w-8 h-8 rounded-full bg-accent/10 border border-accent/20 flex items-center justify-center flex-shrink-0">
        <span className="text-sm font-bold text-accent">{number}</span>
      </div>
      <div>
        <h4 className="text-sm font-medium text-text">{title}</h4>
        <p className="text-xs text-muted mt-0.5">{desc}</p>
      </div>
    </div>
  );
}
