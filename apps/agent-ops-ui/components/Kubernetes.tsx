
import React, { useState } from 'react';
import { 
  Box, Layers, Activity, Filter, RefreshCw, X, Terminal, Globe, ArrowLeft, Network, Cpu, Zap, ShieldCheck, Database
} from 'lucide-react';
import { PieChart, Pie, Cell, ResponsiveContainer } from 'recharts';
import { MOCK_CLUSTER_NODES, MOCK_PODS, TRANSLATIONS } from '../constants';
import { Language, ClusterNode, K8sPod } from '../types';

const Gauge = ({ value, label, color, unit = '%' }: { value: number, label: string, color: string, unit?: string }) => {
  const radius = 30;
  const circumference = 2 * Math.PI * radius;
  const strokeDashoffset = circumference - (Math.min(value, 100) / 100) * circumference;

  return (
    <div className="flex flex-col items-center group/gauge">
      <div className="relative w-16 h-16 group-hover/gauge:scale-110 transition-transform duration-300">
        <svg className="w-full h-full transform -rotate-90">
          <circle cx="32" cy="32" r={radius} stroke="currentColor" strokeWidth="4" fill="transparent" className="text-slate-100 dark:text-nexus-700/30" />
          <circle 
            cx="32" cy="32" r={radius} 
            stroke={color} 
            strokeWidth="5" 
            fill="transparent" 
            strokeDasharray={circumference} 
            strokeDashoffset={strokeDashoffset} 
            strokeLinecap="round"
            className="transition-all duration-1000 ease-out"
          />
        </svg>
        <div className="absolute inset-0 flex items-center justify-center text-[10px] font-black text-slate-900 dark:text-white">
          {Math.round(value)}{unit}
        </div>
      </div>
      <span className="text-[9px] font-black uppercase tracking-[0.2em] text-slate-400 dark:text-nexus-500 mt-3 group-hover/gauge:text-nexus-accent transition-colors">{label}</span>
    </div>
  );
};

const getPosition = (index: number, total: number) => {
  const angle = (index / total) * 2 * Math.PI - Math.PI / 2;
  const radius = 200; // Increased radius for better spacing
  const x = 400 + radius * Math.cos(angle);
  const y = 300 + radius * Math.sin(angle);
  return { x, y };
};

const Kubernetes = ({ lang }: { lang: Language }) => {
  const t = TRANSLATIONS[lang];
  const [activeNamespace, setActiveNamespace] = useState<string | null>(null);
  const [selectedPod, setSelectedPod] = useState<K8sPod | null>(null);
  const [isRefreshing, setIsRefreshing] = useState(false);

  const namespaces = Array.from(new Set(MOCK_PODS.map(p => p.namespace)));
  const totalCpu = MOCK_CLUSTER_NODES.reduce((acc, n) => acc + n.cpu, 0) / MOCK_CLUSTER_NODES.length;
  const donutData = [
    { name: 'Used', value: Math.round(totalCpu) },
    { name: 'Free', value: 100 - Math.round(totalCpu) },
  ];
  const COLORS = ['#3b82f6', 'rgba(148, 163, 184, 0.1)'];

  const handleRefresh = () => {
    setIsRefreshing(true);
    setTimeout(() => setIsRefreshing(false), 1500);
  };

  return (
    <div className="space-y-10 animate-fade-in pb-20 h-full flex flex-col">
      
      {/* Dynamic Header Metrics */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-8">
         <div className="bg-white/80 dark:bg-nexus-800/60 backdrop-blur-xl border border-slate-200 dark:border-nexus-700/50 p-6 rounded-[2rem] flex items-center shadow-sm group hover:border-nexus-accent/30 transition-all duration-500">
            <div className="p-4 bg-blue-500/10 rounded-2xl text-blue-500 mr-6 group-hover:bg-blue-500 group-hover:text-white transition-all shadow-inner group-hover:rotate-12">
               <Layers size={24} />
            </div>
            <div>
               <div className="text-[10px] text-slate-400 dark:text-nexus-500 font-black uppercase tracking-[0.25em]">{t.namespaces}</div>
               <div className="text-3xl font-black text-slate-900 dark:text-white tracking-tighter mt-1">{namespaces.length}</div>
            </div>
         </div>
         <div className="bg-white/80 dark:bg-nexus-800/60 backdrop-blur-xl border border-slate-200 dark:border-nexus-700/50 p-6 rounded-[2rem] flex items-center shadow-sm group hover:border-nexus-accent/30 transition-all duration-500">
            <div className="p-4 bg-purple-500/10 rounded-2xl text-purple-500 mr-6 group-hover:bg-purple-500 group-hover:text-white transition-all shadow-inner group-hover:rotate-12">
               <Box size={24} />
            </div>
            <div>
               <div className="text-[10px] text-slate-400 dark:text-nexus-500 font-black uppercase tracking-[0.25em]">{t.pods}</div>
               <div className="text-3xl font-black text-slate-900 dark:text-white tracking-tighter mt-1">{MOCK_PODS.length}</div>
            </div>
         </div>
         <div className="bg-white/80 dark:bg-nexus-800/60 backdrop-blur-xl border border-slate-200 dark:border-nexus-700/50 p-6 rounded-[2rem] flex items-center shadow-sm group hover:border-nexus-accent/30 transition-all duration-500">
            <div className="h-16 w-16 mr-6 relative group-hover:scale-110 transition-transform">
               <ResponsiveContainer width="100%" height="100%">
                 <PieChart>
                   <Pie data={donutData} innerRadius={22} outerRadius={30} paddingAngle={6} dataKey="value">
                     {donutData.map((entry, index) => (
                       <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                     ))}
                   </Pie>
                 </PieChart>
               </ResponsiveContainer>
               <div className="absolute inset-0 flex items-center justify-center text-[10px] text-slate-900 dark:text-white font-black">{Math.round(totalCpu)}%</div>
            </div>
            <div>
               <div className="text-[10px] text-slate-400 dark:text-nexus-500 font-black uppercase tracking-[0.25em]">Aggregated Load</div>
               <div className="text-[11px] font-bold text-nexus-500 mt-1 uppercase tracking-widest">Core Optimization</div>
            </div>
         </div>
         <div className="bg-white/80 dark:bg-nexus-800/60 backdrop-blur-xl border border-slate-200 dark:border-nexus-700/50 p-6 rounded-[2rem] flex items-center justify-between shadow-sm hover:border-nexus-accent/30 transition-all duration-500">
             <div className="flex items-center">
               <div className="w-3 h-3 bg-green-500 rounded-full animate-pulse mr-4 shadow-[0_0_15px_#10b981]"></div>
               <span className="text-[10px] font-black uppercase tracking-[0.25em] text-slate-700 dark:text-white">MESH_SYNC: OK</span>
             </div>
             <button onClick={handleRefresh} className={`p-3 rounded-2xl bg-slate-100 dark:bg-nexus-900/50 hover:bg-slate-200 dark:hover:bg-nexus-800 transition-all border border-transparent hover:border-nexus-accent/20 ${isRefreshing ? 'animate-spin text-nexus-accent' : 'text-nexus-400'}`}>
                <RefreshCw size={20} />
             </button>
         </div>
      </div>

      {/* Topology Canvas Area */}
      <div className="flex-1 grid grid-cols-1 lg:grid-cols-12 gap-8 min-h-[600px]">
         
         {/* Service Mesh Canvas */}
         <div className="lg:col-span-8 bg-white/70 dark:bg-nexus-800/50 backdrop-blur-xl border border-slate-200 dark:border-nexus-700/50 rounded-[3rem] p-10 relative overflow-hidden flex flex-col shadow-sm group">
            <div className="absolute top-0 right-0 w-80 h-80 bg-nexus-accent/5 rounded-full blur-[100px] -mr-40 -mt-40"></div>
            
            <div className="flex justify-between items-center mb-12 z-10 relative">
               <div>
                  <h3 className="text-sm font-black uppercase tracking-[0.3em] text-slate-900 dark:text-white flex items-center">
                     <Globe className="mr-4 text-nexus-accent" size={20} /> 
                     {activeNamespace ? `DOMAIN: ${activeNamespace.toUpperCase()}` : "CLUSTER TOPOLOGY MATRIX"}
                  </h3>
                  <p className="text-[10px] text-slate-400 dark:text-nexus-500 font-black uppercase mt-2 tracking-[0.2em]">Visualizing Latency-Aware Mesh Connectivity</p>
               </div>
               {activeNamespace && (
                 <button 
                   onClick={() => { setActiveNamespace(null); setSelectedPod(null); }}
                   className="flex items-center text-[10px] font-black uppercase tracking-widest px-6 py-3 bg-nexus-900 text-white dark:bg-nexus-accent rounded-2xl hover:scale-105 transition-all shadow-xl shadow-nexus-accent/20 border border-white/10"
                 >
                   <ArrowLeft size={14} className="mr-3" /> {t.backToMap}
                 </button>
               )}
            </div>

            {/* Mesh Canvas */}
            <div className="flex-1 relative bg-slate-50/50 dark:bg-nexus-900/40 rounded-[2.5rem] border border-dashed border-slate-200 dark:border-nexus-700/50 overflow-hidden shadow-inner group/canvas">
               <div className="absolute inset-0 pointer-events-none opacity-[0.05] dark:opacity-[0.03]" style={{ backgroundImage: 'radial-gradient(circle, #64748b 2px, transparent 2px)', backgroundSize: '40px 40px' }}></div>

               {!activeNamespace && (
                 <div className="grid grid-cols-1 md:grid-cols-3 gap-10 p-12 h-full items-center justify-center overflow-auto animate-fade-in">
                    {namespaces.map((ns) => {
                       const nsPods = MOCK_PODS.filter(p => p.namespace === ns);
                       const errorCount = nsPods.filter(p => p.status === 'Error').length;
                       
                       return (
                          <div 
                             key={ns}
                             onClick={() => setActiveNamespace(ns)}
                             className="relative p-10 rounded-[2.5rem] border-2 border-slate-100 dark:border-nexus-800/80 bg-white/90 dark:bg-nexus-800/80 hover:border-nexus-accent hover:scale-[1.05] transition-all duration-500 cursor-pointer group/card shadow-2xl backdrop-blur-xl"
                          >
                             <div className="flex items-center justify-between mb-8">
                                <div className="p-4 bg-nexus-accent/10 rounded-2xl text-nexus-accent group-hover/card:bg-nexus-accent group-hover/card:text-white transition-all shadow-inner group-hover/card:rotate-12">
                                  <Layers size={28} />
                                </div>
                                {errorCount > 0 && <span className="w-4 h-4 bg-red-500 rounded-full animate-ping shadow-[0_0_15px_#ef4444]"></span>}
                             </div>
                             <h4 className="text-xl font-black text-slate-900 dark:text-white mb-2 tracking-tighter">{ns}</h4>
                             <p className="text-[10px] font-black text-slate-400 dark:text-nexus-500 uppercase tracking-[0.25em]">{nsPods.length} ACTIVE NEURAL PODS</p>
                             
                             <div className="mt-8 flex flex-wrap gap-2">
                                {nsPods.slice(0, 12).map((p, i) => (
                                   <div key={i} className={`w-2.5 h-2.5 rounded-full ${p.status === 'Running' ? 'bg-green-500 shadow-[0_0_8px_#10b981]' : 'bg-red-500 shadow-[0_0_8px_#ef4444]'}`}></div>
                                ))}
                                {nsPods.length > 12 && <span className="text-[10px] text-nexus-accent font-black ml-2">+ {nsPods.length - 12}</span>}
                             </div>
                          </div>
                       );
                    })}
                 </div>
               )}

               {activeNamespace && (
                 <div className="absolute inset-0 animate-fade-in">
                    <svg className="absolute inset-0 w-full h-full pointer-events-none z-0">
                       <defs>
                          <marker id="arrowhead" markerWidth="10" markerHeight="7" refX="35" refY="3.5" orient="auto">
                             <polygon points="0 0, 10 3.5, 0 7" fill="#3b82f6" />
                          </marker>
                       </defs>
                       {MOCK_PODS
                         .filter(p => p.namespace === activeNamespace)
                         .map((pod, i, arr) => {
                            const sourcePos = getPosition(i, arr.length);
                            return pod.connections.map(targetId => {
                               const targetIndex = arr.findIndex(p => p.id === targetId);
                               if (targetIndex === -1) return null;
                               const targetPos = getPosition(targetIndex, arr.length);
                               return (
                                  <path 
                                    key={`${pod.id}-${targetId}`}
                                    d={`M ${sourcePos.x} ${sourcePos.y} C ${(sourcePos.x + targetPos.x)/2 + 80} ${(sourcePos.y + targetPos.y)/2 - 80}, ${(sourcePos.x + targetPos.x)/2 - 80} ${(sourcePos.y + targetPos.y)/2 + 80}, ${targetPos.x} ${targetPos.y}`}
                                    stroke="#3b82f6" strokeWidth="2" strokeDasharray="8,8"
                                    fill="none" markerEnd="url(#arrowhead)"
                                    className="opacity-20 transition-opacity duration-1000"
                                  >
                                     <animate attributeName="stroke-dashoffset" from="16" to="0" dur="2s" repeatCount="indefinite" />
                                  </path>
                               );
                            });
                         })
                       }
                    </svg>

                    {MOCK_PODS
                      .filter(p => p.namespace === activeNamespace)
                      .map((pod, i, arr) => {
                         const pos = getPosition(i, arr.length);
                         return (
                            <div 
                               key={pod.id}
                               onClick={() => setSelectedPod(pod)}
                               style={{ left: pos.x, top: pos.y }}
                               className={`absolute transform -translate-x-1/2 -translate-y-1/2 w-20 h-20 rounded-[1.5rem] border-2 flex flex-col items-center justify-center cursor-pointer transition-all duration-500 hover:scale-125 z-10 shadow-2xl backdrop-blur-xl ${
                                  selectedPod?.id === pod.id 
                                  ? 'border-nexus-accent bg-white dark:bg-nexus-800 ring-[12px] ring-blue-500/10' 
                                  : pod.status === 'Error' ? 'border-red-500 bg-red-500/10' : 'border-slate-200/50 dark:border-nexus-700/50 bg-white/70 dark:bg-nexus-900/70'
                               }`}
                            >
                               <Box size={24} className={selectedPod?.id === pod.id ? 'text-nexus-accent' : 'text-slate-400 dark:text-nexus-500'} />
                               <div className={`absolute -bottom-10 text-[8px] font-black px-3 py-1.5 rounded-xl bg-slate-900/90 text-white truncate max-w-[140px] uppercase tracking-[0.2em] backdrop-blur-xl border border-white/10 shadow-2xl transition-all ${selectedPod?.id === pod.id ? 'scale-110' : ''}`}>
                                  {pod.name.split('-')[0]}
                               </div>
                               
                               <div className={`absolute -top-1 -right-1 w-4 h-4 rounded-full border-2 border-white dark:border-nexus-900 ${pod.status === 'Running' ? 'bg-green-500 shadow-[0_0_12px_#10b981]' : 'bg-red-500 shadow-[0_0_12px_#ef4444]'}`}></div>
                            </div>
                         );
                      })
                    }
                 </div>
               )}
            </div>
         </div>

         {/* Detailed Pod Telemetry Panel */}
         <div className="lg:col-span-4 flex flex-col space-y-8">
            
            {/* Health & eBPF Details */}
            <div className="flex-1 bg-white/70 dark:bg-nexus-800/50 backdrop-blur-xl border border-slate-200 dark:border-nexus-700/50 rounded-[3rem] p-8 shadow-sm flex flex-col group overflow-hidden relative">
               <div className="absolute top-0 right-0 w-40 h-40 bg-nexus-accent/5 rounded-full blur-[80px] -mr-20 -mt-20"></div>
               
               {selectedPod ? (
                 <div className="animate-slide-up h-full flex flex-col overflow-y-auto pr-1 custom-scrollbar relative z-10">
                    <div className="flex justify-between items-start mb-10">
                       <div>
                          <h3 className="text-xl font-black text-slate-900 dark:text-white leading-none tracking-tighter mb-4">
                             {selectedPod.name}
                          </h3>
                          <div className="flex items-center space-x-4">
                             <span className={`px-3 py-1 rounded-xl text-[9px] font-black uppercase tracking-[0.25em] ${
                               selectedPod.status === 'Running' ? 'bg-green-500 text-white shadow-xl shadow-green-500/30' : 'bg-red-500 text-white shadow-xl shadow-red-500/30'
                             }`}>
                                {selectedPod.status}
                             </span>
                             <span className="text-[10px] font-black text-slate-400 dark:text-nexus-500 font-mono uppercase tracking-[0.2em]">POD_ID: {selectedPod.id}</span>
                          </div>
                       </div>
                       <button onClick={() => setSelectedPod(null)} className="p-3 hover:bg-slate-100 dark:hover:bg-nexus-900 rounded-2xl transition-all text-slate-400 hover:text-slate-900 dark:hover:text-white border border-transparent hover:border-nexus-800/50">
                          <X size={20} />
                       </button>
                    </div>

                    <div className="space-y-10">
                       <div className="grid grid-cols-2 gap-6">
                           <div className="p-6 bg-slate-50 dark:bg-nexus-900/50 rounded-[2rem] border border-slate-100 dark:border-nexus-800/50 shadow-inner group/stat">
                              <div className="text-[9px] font-black text-slate-400 dark:text-nexus-500 uppercase tracking-[0.25em] mb-2 group-hover/stat:text-nexus-accent transition-colors">RESTARTS</div>
                              <div className="text-2xl font-mono font-black text-slate-900 dark:text-white leading-none">{selectedPod.restarts}</div>
                           </div>
                           <div className="p-6 bg-slate-50 dark:bg-nexus-900/50 rounded-[2rem] border border-slate-100 dark:border-nexus-800/50 shadow-inner group/stat">
                              <div className="text-[9px] font-black text-slate-400 dark:text-nexus-500 uppercase tracking-[0.25em] mb-2 group-hover/stat:text-nexus-accent transition-colors">RUNTIME_AGE</div>
                              <div className="text-2xl font-mono font-black text-slate-900 dark:text-white leading-none">{selectedPod.age}</div>
                           </div>
                       </div>

                       {/* Advanced eBPF Observability */}
                       <div className="bg-slate-50/50 dark:bg-nexus-900/30 rounded-[2.5rem] p-8 border border-slate-100 dark:border-nexus-700/50 relative overflow-hidden group/ebpf">
                          <div className="flex items-center justify-between mb-8">
                             <div className="flex items-center text-[10px] font-black text-slate-900 dark:text-white uppercase tracking-[0.3em]">
                                <Activity size={16} className="mr-4 text-nexus-accent animate-pulse" /> EBPF TELEMETRY
                             </div>
                             <div className="flex items-center">
                                <span className="w-2 h-2 rounded-full bg-blue-500 animate-ping mr-3"></span>
                                <span className="text-[9px] font-black text-nexus-accent uppercase tracking-[0.25em]">SYSCALL_SYNC</span>
                             </div>
                          </div>
                          
                          {selectedPod.ebpf ? (
                             <div className="space-y-10">
                                <div className="flex justify-between items-center px-2">
                                   <Gauge value={selectedPod.ebpf.httpThroughput} label="HTTP_QPS" unit="" color="#10b981" />
                                   <Gauge value={selectedPod.ebpf.tcpLatency} label="LATENCY" unit="ms" color="#f59e0b" />
                                   <Gauge value={selectedPod.ebpf.networkIn} label="NET_IN" unit="MB" color="#3b82f6" />
                                </div>
                                <div className="space-y-6 pt-8 border-t border-slate-200/50 dark:border-nexus-700/50">
                                   <div className="flex justify-between items-center text-[10px] font-black uppercase tracking-[0.2em]">
                                      <span className="text-slate-400 dark:text-nexus-500 flex items-center"><Zap size={14} className="mr-3 text-yellow-500" /> ACTIVE_SYSCALL</span>
                                      <span className="text-slate-900 dark:text-white font-mono bg-white/50 dark:bg-nexus-900 px-3 py-1 rounded-lg border border-slate-200/50 dark:border-nexus-800 shadow-sm">{selectedPod.ebpf.lastSyscall}</span>
                                   </div>
                                   <div className="flex justify-between items-center text-[10px] font-black uppercase tracking-[0.2em]">
                                      <span className="text-slate-400 dark:text-nexus-500 flex items-center"><Database size={14} className="mr-3 text-blue-500" /> KERNEL_PID</span>
                                      <span className="text-slate-900 dark:text-white font-mono bg-white/50 dark:bg-nexus-900 px-3 py-1 rounded-lg border border-slate-200/50 dark:border-nexus-800 shadow-sm">{selectedPod.ebpf.pid}</span>
                                   </div>
                                </div>
                             </div>
                          ) : (
                             <div className="py-12 flex flex-col items-center justify-center opacity-30 group-hover/ebpf:opacity-50 transition-opacity">
                                <Activity size={40} className="mb-4" />
                                <span className="text-[10px] font-black uppercase tracking-[0.3em]">TELEMETRY_LINK_OFFLINE</span>
                             </div>
                          )}
                       </div>

                       {/* Log Stream Visualization */}
                       <div className="bg-nexus-900 rounded-[2rem] p-6 border border-nexus-800/50 shadow-inner group/logs overflow-hidden relative">
                          <div className="absolute top-0 left-0 w-full h-1 bg-gradient-to-r from-transparent via-nexus-accent/20 to-transparent animate-scan"></div>
                          <h4 className="text-[10px] font-black uppercase text-nexus-500 mb-5 tracking-[0.25em] flex items-center">
                            <Terminal size={12} className="mr-3" /> STREAMING_POD_LOGS
                          </h4>
                          <div className="space-y-3.5 h-36 overflow-y-auto font-mono text-[10px] custom-scrollbar">
                             {selectedPod.logs.map((l, i) => (
                               <div key={i} className="flex space-x-4 opacity-50 hover:opacity-100 transition-opacity duration-300">
                                  <span className="text-nexus-600 shrink-0 font-bold">[{new Date().toLocaleTimeString()}]</span>
                                  <span className="text-nexus-300 break-all leading-relaxed">{l}</span>
                                </div>
                             ))}
                          </div>
                       </div>
                    </div>
                 </div>
               ) : (
                 <div className="h-full flex flex-col items-center justify-center text-slate-300 dark:text-nexus-800/80">
                    <div className="p-10 rounded-full border-4 border-dashed border-current mb-8 animate-pulse">
                      <ShieldCheck size={56} />
                    </div>
                    <p className="text-[11px] font-black uppercase tracking-[0.4em] text-center max-w-[220px] leading-relaxed text-slate-400 dark:text-nexus-500">
                       SELECT_ASSET_FOR_TELEMETRY_LINK
                    </p>
                 </div>
               )}
            </div>

            {/* Micro-Terminal Widget */}
            <div className="h-64 bg-nexus-900 rounded-[2.5rem] border border-nexus-800/80 flex flex-col overflow-hidden shadow-2xl relative">
               <div className="bg-nexus-800/90 p-5 flex items-center justify-between border-b border-nexus-800 backdrop-blur-xl">
                  <div className="flex items-center space-x-4">
                     <Terminal size={16} className="text-nexus-accent" />
                     <span className="text-[10px] font-black text-nexus-300 uppercase tracking-[0.3em]">K8S_PROXY_CLI</span>
                  </div>
                  <div className="flex space-x-2">
                     <div className="w-3 h-3 rounded-full bg-red-500/50 border border-red-400/20"></div>
                     <div className="w-3 h-3 rounded-full bg-yellow-500/50 border border-yellow-400/20"></div>
                     <div className="w-3 h-3 rounded-full bg-green-500/50 border border-green-400/20"></div>
                  </div>
               </div>
               <div className="flex-1 p-6 font-mono text-[11px] overflow-y-auto custom-scrollbar bg-black/40">
                  <div className="text-nexus-accent mb-3 flex items-center">
                    <span className="mr-3">➜</span> 
                    <span className="text-nexus-100">root@cluster_master: kubectl describe pod</span>
                  </div>
                  <div className="text-nexus-400 whitespace-pre leading-loose border-l-2 border-nexus-800 pl-4">
                     {selectedPod ? `NAME: ${selectedPod.name}\nNAMESPACE: ${selectedPod.namespace}\nSTATUS: ${selectedPod.status}\nRESTARTS: ${selectedPod.restarts}\nAGE: ${selectedPod.age}` : 
                     `K8S_CORE: v1.31.1-eks\nREGION: US-EAST-1\nENCRYPTION: AES-256-GCM\nAGENT_COUNT: 42`}
                  </div>
                  <div className="flex items-center mt-4">
                     <span className="text-nexus-accent mr-3 font-black">➜</span>
                     <span className="w-2.5 h-5 bg-nexus-accent animate-pulse shadow-[0_0_8px_#3b82f6]"></span>
                  </div>
               </div>
            </div>
         </div>
      </div>
    </div>
  );
};

export default Kubernetes;
