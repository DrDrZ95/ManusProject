import React, { useEffect, useState } from 'react';
import { 
  Activity, Server, Database, GitBranch, 
  Play, RefreshCw, AlertTriangle, Wifi
} from 'lucide-react';
import { 
  AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer 
} from 'recharts';
import { MOCK_PROJECTS, MOCK_TIMELINE_DATA, TRANSLATIONS } from '../constants';
import { ProjectStatus, Language } from '../types';
import { wsService } from '../services/websocket'; // 引入 WS Service

interface DashboardProps {
  lang: Language;
}

const StatusCard = ({ title, value, subtext, icon: Icon, color, animate }: any) => (
  <div className="bg-light-surface dark:bg-nexus-800 border border-light-border dark:border-nexus-700 rounded-lg p-5 shadow-sm hover:shadow-md transition-all group">
    <div className="flex justify-between items-start mb-4">
      <div>
        <h3 className="text-light-textSec dark:text-nexus-400 text-xs font-semibold uppercase tracking-wider">{title}</h3>
        <p className={`text-2xl font-bold text-light-text dark:text-white mt-1 group-hover:text-nexus-accent transition-colors ${animate ? 'animate-pulse' : ''}`}>{value}</p>
      </div>
      <div className={`p-2 rounded-md bg-opacity-20 ${color} text-white`}>
        <Icon size={20} />
      </div>
    </div>
    <p className="text-light-textSec dark:text-nexus-500 text-xs font-mono">{subtext}</p>
  </div>
);

const ProjectRow: React.FC<{ project: ProjectStatus, t: any }> = ({ project, t }) => (
  <div className="flex items-center justify-between p-4 bg-light-bg/50 dark:bg-nexus-800/50 border-b border-light-border dark:border-nexus-700 last:border-0 hover:bg-white dark:hover:bg-nexus-700/50 transition-colors">
    <div className="flex items-center space-x-4">
      <div className={`w-2 h-2 rounded-full ${
        project.status === 'healthy' ? 'bg-green-500 shadow-[0_0_8px_rgba(16,185,129,0.5)]' : 
        project.status === 'warning' ? 'bg-yellow-500' :
        project.status === 'deploying' ? 'bg-blue-500 animate-pulse' : 'bg-red-500'
      }`} />
      <div>
        <h4 className="font-semibold text-sm text-light-text dark:text-white">{project.name}</h4>
        <div className="flex items-center text-xs text-light-textSec dark:text-nexus-400 space-x-2">
          <GitBranch size={10} />
          <span>{project.repo}</span>
        </div>
      </div>
    </div>
    <div className="flex items-center space-x-6 text-sm">
      <div className="text-right">
        <div className="text-light-textSec dark:text-nexus-400 text-xs">{t.uptime}</div>
        <div className="font-mono text-nexus-600 dark:text-nexus-200">{project.uptime}</div>
      </div>
      <div className="text-right hidden sm:block">
        <div className="text-light-textSec dark:text-nexus-400 text-xs">{t.lastDeploy}</div>
        <div className="font-mono text-nexus-600 dark:text-nexus-200">{project.lastDeployment}</div>
      </div>
      <button className="p-2 hover:bg-nexus-200 dark:hover:bg-nexus-600 rounded text-nexus-500 dark:text-nexus-300 hover:text-nexus-accent dark:hover:text-white transition-colors">
        <RefreshCw size={14} />
      </button>
    </div>
  </div>
);

const Dashboard: React.FC<DashboardProps> = ({ lang }) => {
  const t = TRANSLATIONS[lang];
  const [wsConnected, setWsConnected] = useState(false);
  
  // Real-time states powered by WebSocket
  const [healthMetric, setHealthMetric] = useState({ value: "98.2%", status: "healthy" });
  const [activePods, setActivePods] = useState(248);

  useEffect(() => {
    // 1. Connect to WebSocket
    wsService.connect();
    setWsConnected(true);

    // 2. Subscribe to System Health updates (Observer Pattern)
    const unsubscribeHealth = wsService.subscribe('system_health', (data: any) => {
      // Update UI with real-time data
      const overallHealth = 100 - (data.cpu * 0.2 + data.memory * 0.1); // Fake calculation
      setHealthMetric({
        value: overallHealth.toFixed(1) + "%",
        status: data.status
      });
      setActivePods(data.activePods);
    });

    // 3. Cleanup on unmount
    return () => {
      unsubscribeHealth();
      wsService.disconnect();
      setWsConnected(false);
    };
  }, []);

  return (
    <div className="space-y-6 animate-fade-in pb-20">
      
      {/* WS Status Indicator */}
      <div className="flex justify-end items-center mb-2">
         <div className={`flex items-center text-xs font-mono px-2 py-1 rounded border ${
           wsConnected ? 'bg-green-500/10 border-green-500/30 text-green-500' : 'bg-red-500/10 border-red-500/30 text-red-500'
         }`}>
           <Wifi size={12} className="mr-2" />
           WS: {wsConnected ? 'CONNECTED (MOCK STREAM)' : 'DISCONNECTED'}
         </div>
      </div>

      {/* Quick Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <StatusCard 
          title={t.systemHealth} 
          value={healthMetric.value} 
          subtext={healthMetric.status === 'healthy' ? "All systems operational" : "Minor degradation detected"} 
          icon={Activity} 
          color={healthMetric.status === 'healthy' ? "bg-green-500" : "bg-yellow-500"}
          animate={true} // Visual feedback for real-time update
        />
        <StatusCard 
          title={t.activeClusters} 
          value="12" 
          subtext={`${activePods} pods running`} 
          icon={Server} 
          color="bg-purple-500" 
          animate={true}
        />
        <StatusCard 
          title="Data Pipelines" 
          value="8 Active" 
          subtext="1.2TB processed / hr" 
          icon={Database} 
          color="bg-blue-500" 
        />
        <StatusCard 
          title="Pending Jobs" 
          value="3" 
          subtext="ML Training Queue" 
          icon={Play} 
          color="bg-orange-500" 
        />
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
        
        {/* Main Chart Section */}
        <div className="lg:col-span-8 bg-light-surface dark:bg-nexus-800 border border-light-border dark:border-nexus-700 rounded-lg p-5 shadow-sm">
          <div className="flex justify-between items-center mb-6">
            <h3 className="text-lg font-semibold text-light-text dark:text-white">{t.opsTimeline}</h3>
            <div className="flex space-x-2">
               <span className="text-xs text-light-textSec dark:text-nexus-400 flex items-center"><span className="w-2 h-2 bg-nexus-accent rounded-full mr-1"></span> Ops</span>
            </div>
          </div>
          <div className="h-64 w-full">
            <ResponsiveContainer width="100%" height="100%">
              <AreaChart data={MOCK_TIMELINE_DATA}>
                <defs>
                  <linearGradient id="colorOps" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="5%" stopColor="#3b82f6" stopOpacity={0.3}/>
                    <stop offset="95%" stopColor="#3b82f6" stopOpacity={0}/>
                  </linearGradient>
                </defs>
                <CartesianGrid strokeDasharray="3 3" stroke="#94a3b8" opacity={0.3} vertical={false} />
                <XAxis dataKey="time" stroke="#64748b" fontSize={12} tickLine={false} axisLine={false} />
                <YAxis stroke="#64748b" fontSize={12} tickLine={false} axisLine={false} />
                <Tooltip 
                  contentStyle={{ borderRadius: '8px', border: 'none', boxShadow: '0 4px 6px -1px rgb(0 0 0 / 0.1)' }}
                  itemStyle={{ color: '#64748b' }}
                />
                <Area type="monotone" dataKey="value" stroke="#3b82f6" strokeWidth={2} fillOpacity={1} fill="url(#colorOps)" />
              </AreaChart>
            </ResponsiveContainer>
          </div>
        </div>

        {/* Quick Actions Panel */}
        <div className="lg:col-span-4 bg-light-surface dark:bg-nexus-800 border border-light-border dark:border-nexus-700 rounded-lg p-5 shadow-sm flex flex-col">
          <h3 className="text-lg font-semibold text-light-text dark:text-white mb-4">{t.quickActions}</h3>
          <div className="space-y-3 flex-1 overflow-y-auto pr-1 custom-scrollbar">
            <button className="w-full text-left p-3 rounded bg-nexus-50 dark:bg-nexus-700/30 border border-light-border dark:border-nexus-600 hover:bg-nexus-100 dark:hover:bg-nexus-700 hover:border-nexus-300 dark:hover:border-nexus-500 transition-all flex items-center group">
              <div className="p-2 bg-blue-500/10 dark:bg-blue-500/20 rounded-md text-blue-500 dark:text-blue-400 mr-3 group-hover:bg-blue-500 group-hover:text-white transition-colors">
                <Play size={18} />
              </div>
              <div>
                <div className="text-sm font-medium text-light-text dark:text-white">{t.deployAll}</div>
                <div className="text-xs text-light-textSec dark:text-nexus-400">Updates 3 services</div>
              </div>
            </button>
            
            <button className="w-full text-left p-3 rounded bg-nexus-50 dark:bg-nexus-700/30 border border-light-border dark:border-nexus-600 hover:bg-nexus-100 dark:hover:bg-nexus-700 hover:border-nexus-300 dark:hover:border-nexus-500 transition-all flex items-center group">
              <div className="p-2 bg-purple-500/10 dark:bg-purple-500/20 rounded-md text-purple-500 dark:text-purple-400 mr-3 group-hover:bg-purple-500 group-hover:text-white transition-colors">
                <RefreshCw size={18} />
              </div>
              <div>
                <div className="text-sm font-medium text-light-text dark:text-white">{t.retrain}</div>
                <div className="text-xs text-light-textSec dark:text-nexus-400">Last run: 2d ago</div>
              </div>
            </button>

            <button className="w-full text-left p-3 rounded bg-nexus-50 dark:bg-nexus-700/30 border border-light-border dark:border-nexus-600 hover:bg-nexus-100 dark:hover:bg-nexus-700 hover:border-nexus-300 dark:hover:border-nexus-500 transition-all flex items-center group">
              <div className="p-2 bg-orange-500/10 dark:bg-orange-500/20 rounded-md text-orange-500 dark:text-orange-400 mr-3 group-hover:bg-orange-500 group-hover:text-white transition-colors">
                <Database size={18} />
              </div>
              <div>
                <div className="text-sm font-medium text-light-text dark:text-white">{t.flushCache}</div>
                <div className="text-xs text-light-textSec dark:text-nexus-400">Node: Redis-Primary</div>
              </div>
            </button>

            <button className="w-full text-left p-3 rounded bg-nexus-50 dark:bg-nexus-700/30 border border-light-border dark:border-nexus-600 hover:bg-nexus-100 dark:hover:bg-nexus-700 hover:border-nexus-300 dark:hover:border-nexus-500 transition-all flex items-center group">
              <div className="p-2 bg-red-500/10 dark:bg-red-500/20 rounded-md text-red-500 dark:text-red-400 mr-3 group-hover:bg-red-500 group-hover:text-white transition-colors">
                <AlertTriangle size={18} />
              </div>
              <div>
                <div className="text-sm font-medium text-light-text dark:text-white">{t.emergencyStop}</div>
                <div className="text-xs text-light-textSec dark:text-nexus-400">Halts all training jobs</div>
              </div>
            </button>
          </div>
        </div>
      </div>

      {/* Project Status List */}
      <div className="bg-light-surface dark:bg-nexus-800 border border-light-border dark:border-nexus-700 rounded-lg overflow-hidden shadow-sm">
        <div className="p-4 border-b border-light-border dark:border-nexus-700 flex justify-between items-center bg-nexus-50/80 dark:bg-nexus-800/80 backdrop-blur">
          <h3 className="text-lg font-semibold text-light-text dark:text-white">Project Status Overview</h3>
          <span className="px-2 py-1 bg-nexus-200 dark:bg-nexus-700 rounded text-xs text-nexus-600 dark:text-nexus-300">{MOCK_PROJECTS.length} Projects</span>
        </div>
        <div>
          {MOCK_PROJECTS.map(project => (
            <ProjectRow key={project.id} project={project} t={t} />
          ))}
        </div>
      </div>
    </div>
  );
};

export default Dashboard;