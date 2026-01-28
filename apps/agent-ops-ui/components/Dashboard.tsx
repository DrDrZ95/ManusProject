
import React, { useEffect, useState } from 'react';
import { 
  Activity, Server, Database, GitBranch, 
  Play, RefreshCw, AlertTriangle, Wifi, BrainCircuit, TrendingUp, ShieldCheck, Zap, BarChart3, Globe
} from 'lucide-react';
import { 
  AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, BarChart, Bar, Cell
} from 'recharts';
import { MOCK_TIMELINE_DATA, TRANSLATIONS } from '../constants';
import { ProjectStatus, Language } from '../types';
import { wsService } from '../services/websocket'; 
import { projectService } from '../services/project'; 

interface DashboardProps {
  lang: Language;
}

const Sparkline = ({ data, color }: { data: any[], color: string }) => (
  <div className="h-8 w-16 opacity-30 group-hover:opacity-100 transition-opacity duration-500">
    <ResponsiveContainer width="100%" height="100%">
      <AreaChart data={data.slice(-10)}>
        <Area type="monotone" dataKey="value" stroke={color} fill={color} fillOpacity={0.1} strokeWidth={1.5} />
      </AreaChart>
    </ResponsiveContainer>
  </div>
);

const StatusCard = ({ title, value, subtext, icon: Icon, color, animate, trend, chartData }: any) => (
  <div className="bg-white/80 dark:bg-nexus-800/60 backdrop-blur-md border border-slate-200 dark:border-nexus-700/50 rounded-3xl p-6 shadow-sm hover:shadow-2xl hover:border-nexus-accent/50 transition-all duration-500 group relative overflow-hidden">
    <div className={`absolute -right-6 -top-6 w-24 h-24 rounded-full opacity-[0.03] group-hover:opacity-[0.08] transition-opacity bg-current ${color.replace('bg-', 'text-')}`}></div>
    
    <div className="flex justify-between items-start mb-6 relative z-10">
      <div className={`p-3 rounded-2xl ${color} bg-opacity-10 text-white shadow-inner border border-white/10 group-hover:scale-110 transition-transform`}>
        <Icon size={20} className={color.replace('bg-', 'text-')} />
      </div>
      {chartData && <Sparkline data={chartData} color={color.includes('blue') ? '#3b82f6' : color.includes('green') ? '#10b981' : '#a855f7'} />}
    </div>

    <div className="relative z-10">
      <h3 className="text-slate-400 dark:text-nexus-500 text-[9px] font-black uppercase tracking-[0.25em]">{title}</h3>
      <div className="flex items-baseline space-x-2 mt-1.5">
        <p className={`text-3xl font-black text-slate-900 dark:text-white group-hover:text-nexus-accent transition-colors tracking-tight ${animate ? 'animate-pulse' : ''}`}>{value}</p>
        {trend && (
          <span className={`text-[10px] font-black tracking-widest px-1.5 py-0.5 rounded-lg ${trend > 0 ? 'bg-green-500/10 text-green-500' : 'bg-red-500/10 text-red-500'}`}>
            {trend > 0 ? '↑' : '↓'} {Math.abs(trend)}%
          </span>
        )}
      </div>
      <p className="text-slate-500 dark:text-nexus-400 text-[11px] font-bold mt-2 truncate uppercase tracking-tight">{subtext}</p>
    </div>
  </div>
);

const Dashboard: React.FC<DashboardProps> = ({ lang }) => {
  const t = TRANSLATIONS[lang];
  const [wsConnected, setWsConnected] = useState(false);
  const [projects, setProjects] = useState<ProjectStatus[]>([]);
  const [healthMetric, setHealthMetric] = useState({ value: "98.2%", status: "healthy" });
  const [activePods, setActivePods] = useState(248);

  const resourceData = [
    { name: 'Cluster A', usage: 65, color: '#3b82f6' },
    { name: 'Cluster B', usage: 82, color: '#a855f7' },
    { name: 'Cluster C', usage: 45, color: '#10b981' },
    { name: 'Cluster D', usage: 91, color: '#ef4444' },
    { name: 'Cluster E', usage: 30, color: '#6366f1' },
  ];

  useEffect(() => {
    const fetchData = async () => {
      try {
        const data = await projectService.getProjects();
        setProjects(data);
      } catch (e) {
        console.error("Failed to fetch projects", e);
      }
    };
    fetchData();
    wsService.connect();
    setWsConnected(true);

    const unsubscribeHealth = wsService.subscribe('system_health', (data: any) => {
      const overallHealth = 100 - (data.cpu * 0.2 + data.memory * 0.1); 
      setHealthMetric({
        value: overallHealth.toFixed(1) + "%",
        status: data.status
      });
      setActivePods(data.activePods);
    });

    return () => {
      unsubscribeHealth();
      wsService.disconnect();
      setWsConnected(false);
    };
  }, []);

  return (
    <div className="space-y-10 animate-fade-in pb-20">
      
      {/* Dynamic Header */}
      <div className="flex flex-col lg:flex-row lg:justify-between lg:items-end space-y-6 lg:space-y-0">
         <div className="relative">
            <div className="absolute -left-6 top-1 bottom-1 w-1.5 bg-nexus-accent rounded-full hidden md:block opacity-50"></div>
            <h2 className="text-4xl font-black text-slate-900 dark:text-white tracking-tighter flex items-center">
              Manus<span className="text-nexus-accent font-light mx-2">Project</span> 
              <span className="text-slate-200 dark:text-nexus-800 mx-4 font-thin">/</span> 
              <span className="text-slate-400 dark:text-nexus-500 font-bold text-xl uppercase tracking-[0.3em]">Intelligence Hub</span>
            </h2>
            <div className="flex items-center mt-3 space-x-6">
              <p className="text-slate-500 dark:text-nexus-500 text-[10px] font-black uppercase tracking-widest flex items-center">
                <span className="w-2 h-2 rounded-full bg-nexus-accent mr-3 animate-ping"></span>
                Session ID: <span className="text-slate-900 dark:text-white ml-2 font-mono">X-7742-ALPHA</span>
              </p>
              <div className="w-1.5 h-1.5 rounded-full bg-slate-300 dark:bg-nexus-800"></div>
              <p className="text-slate-500 dark:text-nexus-500 text-[10px] font-black uppercase tracking-widest">Operator: <span className="text-slate-900 dark:text-white ml-2">ADMIN.SECURE</span></p>
            </div>
         </div>
         <div className="flex items-center space-x-4">
            <div className={`flex items-center px-6 py-3 rounded-2xl border backdrop-blur-sm transition-all duration-700 shadow-sm ${
              wsConnected ? 'bg-green-500/5 border-green-500/20 text-green-600 dark:text-green-400' : 'bg-red-500/5 border-red-500/20 text-red-500'
            }`}>
              <div className={`w-2.5 h-2.5 rounded-full mr-4 ${wsConnected ? 'bg-green-500 shadow-[0_0_10px_#10b981]' : 'bg-red-500 shadow-[0_0_10px_#ef4444]'}`}></div>
              <span className="text-[10px] font-black uppercase tracking-[0.25em]">Telemetry Stream: {wsConnected ? 'ACTIVE' : 'STANDBY'}</span>
            </div>
            <button className="flex items-center space-x-3 px-6 py-3 bg-nexus-900 dark:bg-nexus-accent text-white rounded-2xl font-black text-[10px] uppercase tracking-[0.25em] hover:scale-105 transition-all shadow-2xl active:scale-95 border border-white/10">
               <TrendingUp size={16} />
               <span>Analytics Forge</span>
            </button>
         </div>
      </div>

      {/* Main Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8">
        <StatusCard 
          title={t.systemHealth} 
          value={healthMetric.value} 
          subtext={healthMetric.status === 'healthy' ? "CORE STABILITY NOMINAL" : "NODE LATENCY DETECTED"} 
          icon={ShieldCheck} 
          color="bg-green-500"
          animate={healthMetric.status !== 'healthy'}
          trend={1.2}
          chartData={MOCK_TIMELINE_DATA}
        />
        <StatusCard 
          title="Cluster Efficiency" 
          value="84.2%" 
          subtext={`${activePods} ACTIVE MODEL PODS`} 
          icon={Server} 
          color="bg-purple-500" 
          trend={-2.4}
          chartData={MOCK_TIMELINE_DATA}
        />
        <StatusCard 
          title="Neural Throughput" 
          value="142.8 GB/s" 
          subtext="AGGREGATE PACKET RATE" 
          icon={BrainCircuit} 
          color="bg-blue-500" 
          trend={8.5}
          chartData={MOCK_TIMELINE_DATA}
        />
        <StatusCard 
          title="Orchestration" 
          value="07" 
          subtext="PENDING AUTOMATION JOBS" 
          icon={Zap} 
          color="bg-orange-500" 
          animate={true}
          chartData={MOCK_TIMELINE_DATA}
        />
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-8">
        
        {/* Network Throughput Area Chart */}
        <div className="lg:col-span-8 bg-white/80 dark:bg-nexus-800/60 backdrop-blur-md border border-slate-200 dark:border-nexus-700/50 rounded-[2.5rem] p-10 shadow-sm group">
          <div className="flex justify-between items-center mb-12">
            <div>
               <h3 className="text-xs font-black uppercase tracking-[0.25em] text-slate-800 dark:text-white flex items-center">
                  <Activity size={16} className="mr-4 text-nexus-accent" />
                  {t.opsTimeline}
               </h3>
               <p className="text-[10px] text-slate-400 dark:text-nexus-500 font-bold uppercase mt-2 tracking-[0.2em]">Global Telemetry Signal Processing</p>
            </div>
            <div className="flex bg-slate-100 dark:bg-nexus-900 p-1.5 rounded-2xl shadow-inner border border-slate-200/50 dark:border-nexus-800">
               <button className="px-5 py-2 text-[10px] font-black uppercase tracking-widest text-nexus-accent bg-white dark:bg-nexus-800 rounded-xl shadow-lg border border-slate-100 dark:border-nexus-700">24H</button>
               <button className="px-5 py-2 text-[10px] font-black uppercase tracking-widest text-slate-400 hover:text-nexus-accent transition-colors">7D</button>
            </div>
          </div>
          <div className="h-96 w-full">
            <ResponsiveContainer width="100%" height="100%">
              <AreaChart data={MOCK_TIMELINE_DATA}>
                <defs>
                  <linearGradient id="colorUsage" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="5%" stopColor="#3b82f6" stopOpacity={0.25}/>
                    <stop offset="95%" stopColor="#3b82f6" stopOpacity={0}/>
                  </linearGradient>
                </defs>
                <CartesianGrid strokeDasharray="6 6" stroke="#94a3b8" opacity={0.08} vertical={false} />
                <XAxis dataKey="time" stroke="#64748b" fontSize={10} tickLine={false} axisLine={false} dy={15} tick={{fontWeight: 700}} />
                <YAxis stroke="#64748b" fontSize={10} tickLine={false} axisLine={false} tickFormatter={(val) => `${val}%`} tick={{fontWeight: 700}} />
                <Tooltip 
                  cursor={{ stroke: '#3b82f6', strokeWidth: 2, strokeDasharray: '4 4' }}
                  contentStyle={{ backgroundColor: '#0f172a', borderRadius: '24px', border: '1px solid rgba(255,255,255,0.1)', boxShadow: '0 25px 50px -12px rgba(0,0,0,0.7)', fontSize: '12px', color: '#fff', padding: '20px' }}
                />
                <Area type="monotone" dataKey="value" stroke="#3b82f6" strokeWidth={4} fillOpacity={1} fill="url(#colorUsage)" animationDuration={3000} strokeLinecap="round" />
              </AreaChart>
            </ResponsiveContainer>
          </div>
        </div>

        {/* Resource Distribution Chart */}
        <div className="lg:col-span-4 bg-white/80 dark:bg-nexus-800/60 backdrop-blur-md border border-slate-200 dark:border-nexus-700/50 rounded-[2.5rem] p-10 shadow-sm flex flex-col">
          <h3 className="text-xs font-black uppercase tracking-[0.25em] text-slate-800 dark:text-white mb-10 flex items-center">
            <BarChart3 size={16} className="mr-4 text-purple-500" />
            Load Optimization
          </h3>
          <div className="flex-1 min-h-[300px]">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={resourceData} layout="vertical" margin={{ left: -20 }}>
                <XAxis type="number" hide />
                <YAxis dataKey="name" type="category" stroke="#64748b" fontSize={10} axisLine={false} tickLine={false} tick={{fontWeight: 700}} />
                <Tooltip 
                  cursor={{ fill: 'transparent' }}
                  contentStyle={{ backgroundColor: '#1e293b', border: 'none', borderRadius: '16px', boxShadow: '0 10px 30px rgba(0,0,0,0.3)' }}
                />
                <Bar dataKey="usage" radius={[0, 8, 8, 0]} barSize={16}>
                  {resourceData.map((entry, index) => (
                    <Cell key={`cell-${index}`} fill={entry.color} />
                  ))}
                </Bar>
              </BarChart>
            </ResponsiveContainer>
          </div>
          <div className="mt-8 pt-8 border-t border-slate-100 dark:border-nexus-700/50">
             <div className="flex items-center justify-between text-[10px] font-black uppercase tracking-[0.25em] text-slate-500 dark:text-nexus-500 mb-5">
                <span className="flex items-center"><Globe size={14} className="mr-3 text-nexus-accent" /> Regional Matrix</span>
                <span className="text-nexus-accent px-2 py-1 bg-nexus-accent/10 rounded-lg">SYNCHRONIZED</span>
             </div>
             <div className="space-y-4">
                <div className="flex justify-between text-[11px] text-slate-400 dark:text-nexus-400 font-black uppercase tracking-widest">
                   <span>APAC REGION</span>
                   <span className="text-nexus-accent">85% LOAD</span>
                </div>
                <div className="w-full bg-slate-100 dark:bg-nexus-900 rounded-full h-2 shadow-inner">
                   <div className="bg-nexus-accent h-full w-[85%] rounded-full shadow-[0_0_15px_rgba(59,130,246,0.5)] transition-all duration-1000"></div>
                </div>
             </div>
          </div>
        </div>
      </div>

      {/* Quick Action Dock */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
         <button className="flex items-center p-8 bg-gradient-to-br from-blue-600 to-blue-800 rounded-[2.5rem] text-white shadow-2xl shadow-blue-500/30 hover:scale-[1.03] transition-all duration-500 group text-left relative overflow-hidden border border-white/10">
            <div className="absolute top-0 right-0 w-48 h-48 bg-white/10 -mr-20 -mt-20 rounded-full blur-[60px] group-hover:scale-150 transition-transform"></div>
            <div className="bg-white/20 p-5 rounded-[2rem] mr-6 backdrop-blur-xl border border-white/20 shadow-xl group-hover:rotate-12 transition-transform">
               <Zap size={28} className="fill-current" />
            </div>
            <div>
               <h4 className="font-black text-sm uppercase tracking-[0.3em]">{t.deployAll}</h4>
               <p className="text-white/60 text-[10px] mt-2 font-black uppercase tracking-widest leading-relaxed">INSTANT GLOBAL CLUSTER SYNC</p>
            </div>
         </button>

         <button className="flex items-center p-8 bg-white dark:bg-nexus-800/40 backdrop-blur-md border border-slate-200 dark:border-nexus-700/50 rounded-[2.5rem] shadow-sm hover:shadow-2xl hover:border-purple-500/50 transition-all duration-500 group text-left">
            <div className="bg-purple-500/10 p-5 rounded-[2rem] mr-6 group-hover:bg-purple-500 group-hover:text-white transition-all shadow-inner border border-purple-500/10 group-hover:rotate-12">
               <BrainCircuit size={28} className="text-purple-500 group-hover:text-white" />
            </div>
            <div>
               <h4 className="font-black text-sm uppercase tracking-[0.3em] text-slate-900 dark:text-white">{t.retrain}</h4>
               <p className="text-slate-400 dark:text-nexus-500 text-[10px] mt-2 font-black uppercase tracking-widest leading-relaxed">RE-ALIGN NEURAL PARAMETERS</p>
            </div>
         </button>

         <button className="flex items-center p-8 bg-white dark:bg-nexus-800/40 backdrop-blur-md border border-slate-200 dark:border-nexus-700/50 rounded-[2.5rem] shadow-sm hover:shadow-2xl hover:border-red-500/50 transition-all duration-500 group text-left">
            <div className="bg-red-500/10 p-5 rounded-[2rem] mr-6 group-hover:bg-red-500 transition-all shadow-inner border border-red-500/10 group-hover:rotate-12">
               <AlertTriangle size={28} className="text-red-500 group-hover:text-white" />
            </div>
            <div>
               <h4 className="font-black text-sm uppercase tracking-[0.3em] text-slate-900 dark:text-white">{t.emergencyStop}</h4>
               <p className="text-red-500 text-[10px] mt-2 font-black uppercase tracking-widest leading-relaxed">HARD FREEZE ALL PIPELINES</p>
            </div>
         </button>
      </div>
    </div>
  );
};

export default Dashboard;
