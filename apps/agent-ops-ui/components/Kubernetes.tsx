import React, { useState } from 'react';
import { 
  Box, Layers, Activity, Filter, RefreshCw, X, Terminal, Globe, ArrowLeft, Network, Cpu, Zap
} from 'lucide-react';
import { PieChart, Pie, Cell, ResponsiveContainer } from 'recharts';
import { MOCK_CLUSTER_NODES, MOCK_PODS, TRANSLATIONS } from '../constants';
import { Language, ClusterNode, K8sPod } from '../types';

// Custom Circular Gauge Component
const Gauge = ({ value, label, color, unit = '%' }: { value: number, label: string, color: string, unit?: string }) => {
  const radius = 30;
  const circumference = 2 * Math.PI * radius;
  const strokeDashoffset = circumference - (Math.min(value, 100) / 100) * circumference;

  return (
    <div className="flex flex-col items-center">
      <div className="relative w-20 h-20">
        <svg className="w-full h-full transform -rotate-90">
          <circle cx="40" cy="40" r={radius} stroke="currentColor" strokeWidth="6" fill="transparent" className="text-nexus-200 dark:text-nexus-700" />
          <circle 
            cx="40" cy="40" r={radius} 
            stroke={color} 
            strokeWidth="6" 
            fill="transparent" 
            strokeDasharray={circumference} 
            strokeDashoffset={strokeDashoffset} 
            strokeLinecap="round"
            className="transition-all duration-1000 ease-out"
          />
        </svg>
        <div className="absolute inset-0 flex items-center justify-center text-sm font-bold text-light-text dark:text-white">
          {Math.round(value)}{unit}
        </div>
      </div>
      <span className="text-xs text-light-textSec dark:text-nexus-400 mt-1">{label}</span>
    </div>
  );
};

// Helper to calculate random positions for demo
const getPosition = (index: number, total: number) => {
  const angle = (index / total) * 2 * Math.PI;
  const radius = 120; // Distance from center
  const x = 300 + radius * Math.cos(angle);
  const y = 200 + radius * Math.sin(angle);
  return { x, y };
};

const Kubernetes = ({ lang }: { lang: Language }) => {
  const t = TRANSLATIONS[lang];
  const [activeNamespace, setActiveNamespace] = useState<string | null>(null);
  const [selectedPod, setSelectedPod] = useState<K8sPod | null>(null);
  const [isRefreshing, setIsRefreshing] = useState(false);

  // Group pods by namespace for the initial view
  const namespaces = Array.from(new Set(MOCK_PODS.map(p => p.namespace)));
  
  // KPI Data
  const totalCpu = MOCK_CLUSTER_NODES.reduce((acc, n) => acc + n.cpu, 0) / MOCK_CLUSTER_NODES.length;
  const donutData = [
    { name: 'Used', value: Math.round(totalCpu) },
    { name: 'Free', value: 100 - Math.round(totalCpu) },
  ];
  const COLORS = ['#3b82f6', '#1e293b'];

  const handleRefresh = () => {
    setIsRefreshing(true);
    setTimeout(() => setIsRefreshing(false), 1500);
  };

  return (
    <div className="space-y-6 pb-20 h-full flex flex-col">
      
      {/* Top Stats Bar */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4 animate-fade-in">
         <div className="bg-light-surface dark:bg-nexus-800 border border-light-border dark:border-nexus-700 p-4 rounded-lg flex items-center shadow-sm">
            <div className="p-3 bg-blue-500/10 rounded-full text-blue-500 mr-4">
               <Layers size={24} />
            </div>
            <div>
               <div className="text-xs text-light-textSec dark:text-nexus-400 font-bold uppercase">{t.namespaces}</div>
               <div className="text-2xl font-bold text-light-text dark:text-white">{namespaces.length}</div>
            </div>
         </div>
         <div className="bg-light-surface dark:bg-nexus-800 border border-light-border dark:border-nexus-700 p-4 rounded-lg flex items-center shadow-sm">
            <div className="p-3 bg-purple-500/10 rounded-full text-purple-500 mr-4">
               <Box size={24} />
            </div>
            <div>
               <div className="text-xs text-light-textSec dark:text-nexus-400 font-bold uppercase">{t.pods}</div>
               <div className="text-2xl font-bold text-light-text dark:text-white">{MOCK_PODS.length}</div>
            </div>
         </div>
         <div className="bg-light-surface dark:bg-nexus-800 border border-light-border dark:border-nexus-700 p-4 rounded-lg flex items-center shadow-sm">
            <div className="h-16 w-16 mr-4 relative">
               <ResponsiveContainer width="100%" height="100%">
                 <PieChart>
                   <Pie data={donutData} innerRadius={20} outerRadius={30} paddingAngle={5} dataKey="value">
                     {donutData.map((entry, index) => (
                       <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                     ))}
                   </Pie>
                 </PieChart>
               </ResponsiveContainer>
               <div className="absolute inset-0 flex items-center justify-center text-[10px] text-light-text dark:text-white font-bold">{Math.round(totalCpu)}%</div>
            </div>
            <div>
               <div className="text-xs text-light-textSec dark:text-nexus-400 font-bold uppercase">Cluster CPU</div>
               <div className="text-sm text-nexus-500">Avg Utilization</div>
            </div>
         </div>
         <div className="bg-light-surface dark:bg-nexus-800 border border-light-border dark:border-nexus-700 p-4 rounded-lg flex items-center justify-between shadow-sm">
             <div className="flex items-center">
               <div className="w-3 h-3 bg-green-500 rounded-full animate-pulse mr-2"></div>
               <span className="text-sm font-medium text-light-text dark:text-white">System Healthy</span>
             </div>
             <button onClick={handleRefresh} className={`p-2 rounded-full hover:bg-nexus-100 dark:hover:bg-nexus-700 transition-colors ${isRefreshing ? 'animate-spin text-nexus-accent' : 'text-nexus-400'}`}>
                <RefreshCw size={18} />
             </button>
         </div>
      </div>

      {/* Main Content Area */}
      <div className="flex-1 grid grid-cols-1 lg:grid-cols-3 gap-6 min-h-[500px]">
         
         {/* Visual Topology Canvas */}
         <div className="lg:col-span-2 bg-light-surface dark:bg-nexus-800 border border-light-border dark:border-nexus-700 rounded-lg p-6 relative overflow-hidden flex flex-col shadow-sm">
            <div className="flex justify-between items-center mb-6 z-10">
               <h3 className="text-lg font-bold text-light-text dark:text-white flex items-center">
                  <Globe className="mr-2 text-nexus-accent" /> 
                  {activeNamespace ? `${t.serviceMesh}: ${activeNamespace}` : t.topology}
               </h3>
               {activeNamespace && (
                 <button 
                   onClick={() => { setActiveNamespace(null); setSelectedPod(null); }}
                   className="flex items-center text-xs px-3 py-1.5 bg-nexus-100 dark:bg-nexus-700 rounded hover:bg-nexus-200 dark:hover:bg-nexus-600 transition-colors"
                 >
                   <ArrowLeft size={12} className="mr-1" /> {t.backToMap}
                 </button>
               )}
            </div>

            {/* Canvas Area */}
            <div className="flex-1 relative bg-nexus-50 dark:bg-nexus-900/50 rounded border border-dashed border-light-border dark:border-nexus-700 overflow-hidden">
               
               {/* Background Grid Lines (Decorative) */}
               <div className="absolute inset-0 pointer-events-none opacity-5" style={{ backgroundImage: 'radial-gradient(circle, #64748b 1px, transparent 1px)', backgroundSize: '20px 20px' }}></div>

               {/* View 1: Namespace Overview */}
               {!activeNamespace && (
                 <div className="grid grid-cols-2 md:grid-cols-3 gap-8 p-10 h-full items-center justify-center overflow-auto animate-fade-in">
                    {namespaces.map((ns) => {
                       const nsPods = MOCK_PODS.filter(p => p.namespace === ns);
                       const errorCount = nsPods.filter(p => p.status === 'Error').length;
                       
                       return (
                          <div 
                             key={ns}
                             onClick={() => setActiveNamespace(ns)}
                             className="relative p-6 rounded-xl border-2 border-light-border dark:border-nexus-600 bg-white dark:bg-nexus-800 hover:border-nexus-accent hover:scale-105 transition-all cursor-pointer group shadow-lg"
                          >
                             <div className="flex items-center justify-between mb-4">
                                <Layers size={24} className="text-nexus-500 group-hover:text-nexus-accent" />
                                {errorCount > 0 && <span className="w-3 h-3 bg-red-500 rounded-full animate-pulse"></span>}
                             </div>
                             <h4 className="text-lg font-bold text-light-text dark:text-white mb-1">{ns}</h4>
                             <p className="text-sm text-light-textSec dark:text-nexus-400">{nsPods.length} pods running</p>
                             
                             <div className="mt-4 flex space-x-1">
                                {nsPods.slice(0, 5).map((p, i) => (
                                   <div key={i} className={`w-2 h-2 rounded-full ${p.status === 'Running' ? 'bg-green-500' : 'bg-red-500'}`}></div>
                                ))}
                                {nsPods.length > 5 && <span className="text-[10px] text-nexus-400">...</span>}
                             </div>
                          </div>
                       );
                    })}
                 </div>
               )}

               {/* View 2: Pods Canvas */}
               {activeNamespace && (
                 <div className="absolute inset-0 animate-fade-in">
                    <svg className="absolute inset-0 w-full h-full pointer-events-none z-0">
                       <defs>
                          <marker id="arrowhead" markerWidth="10" markerHeight="7" refX="28" refY="3.5" orient="auto">
                             <polygon points="0 0, 10 3.5, 0 7" fill="#64748b" />
                          </marker>
                       </defs>
                       {/* Draw Connections */}
                       {MOCK_PODS
                         .filter(p => p.namespace === activeNamespace)
                         .map((pod, i, arr) => {
                            const sourcePos = getPosition(i, arr.length);
                            return pod.connections.map(targetId => {
                               const targetIndex = arr.findIndex(p => p.id === targetId);
                               if (targetIndex === -1) return null;
                               const targetPos = getPosition(targetIndex, arr.length);
                               return (
                                  <line 
                                    key={`${pod.id}-${targetId}`}
                                    x1={sourcePos.x} y1={sourcePos.y}
                                    x2={targetPos.x} y2={targetPos.y}
                                    stroke="#64748b" strokeWidth="1.5" strokeDasharray="5,5"
                                    markerEnd="url(#arrowhead)"
                                    className="opacity-50"
                                  >
                                     <animate attributeName="stroke-dashoffset" from="10" to="0" dur="1s" repeatCount="indefinite" />
                                  </line>
                               );
                            });
                         })
                       }
                    </svg>

                    {/* Draw Pods */}
                    {MOCK_PODS
                      .filter(p => p.namespace === activeNamespace)
                      .map((pod, i, arr) => {
                         const pos = getPosition(i, arr.length);
                         return (
                            <div 
                               key={pod.id}
                               onClick={() => setSelectedPod(pod)}
                               style={{ left: pos.x, top: pos.y }}
                               className={`absolute transform -translate-x-1/2 -translate-y-1/2 w-16 h-16 rounded-full border-2 flex flex-col items-center justify-center cursor-pointer transition-all hover:scale-110 z-10 shadow-xl ${
                                  selectedPod?.id === pod.id 
                                  ? 'border-nexus-accent bg-nexus-900 ring-4 ring-blue-500/20' 
                                  : pod.status === 'Error' ? 'border-red-500 bg-red-900/20' : 'border-nexus-500 bg-nexus-800'
                               }`}
                            >
                               <Box size={24} className={selectedPod?.id === pod.id ? 'text-nexus-accent' : 'text-white'} />
                               <div className={`absolute -bottom-6 text-[10px] font-bold px-2 py-0.5 rounded bg-black/60 text-white truncate max-w-[120px] backdrop-blur`}>
                                  {pod.name}
                               </div>
                               
                               {/* Status Indicator */}
                               <div className={`absolute top-0 right-0 w-3 h-3 rounded-full border-2 border-nexus-800 ${pod.status === 'Running' ? 'bg-green-500' : 'bg-red-500'}`}></div>
                            </div>
                         );
                      })
                    }
                 </div>
               )}
            </div>
         </div>

         {/* Detail Panel & CLI Widget */}
         <div className="flex flex-col gap-6">
            
            {/* Details Panel */}
            <div className="flex-1 bg-light-surface dark:bg-nexus-800 border border-light-border dark:border-nexus-700 rounded-lg p-6 shadow-sm relative overflow-hidden flex flex-col">
               {selectedPod ? (
                 <div className="animate-slide-up h-full flex flex-col overflow-y-auto pr-1">
                    <div className="flex justify-between items-start mb-4">
                       <div>
                          <h3 className="text-md font-bold text-light-text dark:text-white break-words w-48 leading-tight">
                             {selectedPod.name}
                          </h3>
                          <div className="flex items-center space-x-2 mt-2">
                             <span className={`px-2 py-0.5 rounded text-[10px] font-bold uppercase ${
                               selectedPod.status === 'Running' ? 'bg-green-500/10 text-green-500' : 'bg-red-500/10 text-red-500'
                             }`}>
                                {selectedPod.status}
                             </span>
                             <span className="text-xs text-nexus-500 font-mono">ID: {selectedPod.id}</span>
                          </div>
                       </div>
                       <button onClick={() => setSelectedPod(null)} className="text-nexus-400 hover:text-white">
                          <X size={16} />
                       </button>
                    </div>

                    <div className="space-y-6">
                       {/* Basic Info */}
                       <div className="grid grid-cols-2 gap-2 text-xs text-light-textSec dark:text-nexus-400">
                           <div className="p-2 bg-nexus-50 dark:bg-nexus-900/50 rounded border border-light-border dark:border-nexus-700">
                              <div className="uppercase font-bold mb-1">Restarts</div>
                              <div className="text-light-text dark:text-white">{selectedPod.restarts}</div>
                           </div>
                           <div className="p-2 bg-nexus-50 dark:bg-nexus-900/50 rounded border border-light-border dark:border-nexus-700">
                              <div className="uppercase font-bold mb-1">Age</div>
                              <div className="text-light-text dark:text-white">{selectedPod.age}</div>
                           </div>
                       </div>

                       {/* eBPF Metrics */}
                       <div className="bg-nexus-50 dark:bg-nexus-900/30 rounded-lg p-3 border border-light-border dark:border-nexus-700">
                          <div className="flex items-center justify-between mb-3 border-b border-nexus-700 pb-2">
                             <div className="flex items-center text-xs font-bold text-light-text dark:text-white uppercase">
                                <Activity size={12} className="mr-1 text-nexus-accent" /> {t.ebpfMetrics}
                             </div>
                             <span className="px-1.5 py-0.5 rounded bg-blue-500/20 text-blue-400 text-[10px] font-mono">Live</span>
                          </div>
                          
                          {selectedPod.ebpf ? (
                             <div className="space-y-4">
                                <div className="flex justify-between items-center">
                                   <Gauge value={selectedPod.ebpf.httpThroughput} label="HTTP QPS" unit="" color="#10b981" />
                                   <Gauge value={selectedPod.ebpf.tcpLatency} label="Latency (ms)" unit="" color="#f59e0b" />
                                   <Gauge value={selectedPod.ebpf.pid} label="PID" unit="" color="#64748b" />
                                </div>
                                <div className="space-y-2">
                                   <div className="flex justify-between text-xs">
                                      <span className="text-nexus-400">Net Inbound</span>
                                      <span className="text-white font-mono">{selectedPod.ebpf.networkIn} MB/s</span>
                                   </div>
                                   <div className="w-full bg-nexus-700 h-1.5 rounded-full">
                                      <div className="bg-blue-500 h-1.5 rounded-full" style={{ width: `${Math.min(selectedPod.ebpf.networkIn * 2, 100)}%` }}></div>
                                   </div>
                                   
                                   <div className="flex justify-between text-xs mt-2">
                                      <span className="text-nexus-400">Net Outbound</span>
                                      <span className="text-white font-mono">{selectedPod.ebpf.networkOut} MB/s</span>
                                   </div>
                                   <div className="w-full bg-nexus-700 h-1.5 rounded-full">
                                      <div className="bg-purple-500 h-1.5 rounded-full" style={{ width: `${Math.min(selectedPod.ebpf.networkOut * 2, 100)}%` }}></div>
                                   </div>
                                </div>
                                <div className="pt-2 text-[10px] text-nexus-500 font-mono text-center">
                                   Last Syscall: <span className="text-nexus-300">{selectedPod.ebpf.lastSyscall}</span>
                                </div>
                             </div>
                          ) : (
                             <div className="text-xs text-center text-nexus-500 py-4">No eBPF agent attached</div>
                          )}
                       </div>

                       {/* Logs */}
                       <div>
                          <h4 className="text-xs font-bold uppercase text-light-textSec dark:text-nexus-400 mb-2">Live Logs</h4>
                          <div className="bg-black rounded-lg p-3 font-mono text-[10px] text-green-400 h-32 overflow-y-auto border border-nexus-700 shadow-inner">
                             {selectedPod.logs.map((l, i) => (
                               <div key={i} className="mb-1 border-b border-white/5 pb-0.5">
                                  <span className="opacity-50 mr-2">[{new Date().toLocaleTimeString()}]</span>
                                  {l}
                               </div>
                             ))}
                          </div>
                       </div>
                    </div>
                 </div>
               ) : (
                 <div className="h-full flex flex-col items-center justify-center text-nexus-400 opacity-50">
                    <Network size={48} className="mb-4" />
                    <p className="text-sm text-center">
                       {activeNamespace ? "Select a Pod to view eBPF metrics" : "Select a Namespace to explore mesh"}
                    </p>
                 </div>
               )}
            </div>

            {/* Embedded CLI Widget */}
            <div className="h-48 bg-nexus-900 rounded-lg border border-nexus-700 flex flex-col overflow-hidden shadow-lg">
               <div className="bg-nexus-800 p-2 flex items-center justify-between border-b border-nexus-700">
                  <div className="flex items-center space-x-2">
                     <Terminal size={12} className="text-nexus-400" />
                     <span className="text-[10px] font-bold text-nexus-300 uppercase">Kubectl Console</span>
                  </div>
                  <div className="flex space-x-1">
                     <div className="w-2 h-2 rounded-full bg-red-500"></div>
                     <div className="w-2 h-2 rounded-full bg-yellow-500"></div>
                     <div className="w-2 h-2 rounded-full bg-green-500"></div>
                  </div>
               </div>
               <div className="flex-1 p-3 font-mono text-xs overflow-y-auto custom-scrollbar">
                  <div className="text-green-400 mb-1">➜ ~ kubectl get pods -n {activeNamespace || 'all'}</div>
                  <div className="text-white whitespace-pre-wrap">
                     {activeNamespace ? `NAME                             READY   STATUS    RESTARTS   AGE
${MOCK_PODS.filter(p => p.namespace === activeNamespace).map(p => `${p.name.substring(0, 30).padEnd(32)} 1/1     ${p.status.padEnd(9)} ${p.restarts.toString().padEnd(10)} ${p.age}`).join('\n')}` : 
`NAME           STATUS   ROLES    AGE
k8s-master-01  Ready    master   12d
k8s-worker-01  Ready    worker   12d`}
                  </div>
                  <div className="flex items-center mt-2">
                     <span className="text-green-400 mr-2">➜ ~</span>
                     <span className="text-nexus-500 animate-pulse">_</span>
                  </div>
               </div>
            </div>
         </div>
      </div>
    </div>
  );
};

export default Kubernetes;