
import React, { useState } from 'react';
import { 
  Play, Download, Sliders, CheckCircle, Clock, AlertTriangle, 
  Terminal as TerminalIcon, Filter, Search, ChevronDown, ChevronRight, Activity, Zap, Cpu
} from 'lucide-react';
import { 
  LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer 
} from 'recharts';
import { MOCK_FLOW_NODES, MOCK_LOGS, TRANSLATIONS } from '../constants';
import { Language } from '../types';

interface MLOpsProps {
  lang: Language;
}

const MLOps: React.FC<MLOpsProps> = ({ lang }) => {
  const t = TRANSLATIONS[lang];
  const [logs, setLogs] = useState(MOCK_LOGS);
  const [filterLevel, setFilterLevel] = useState<'ALL' | 'ERROR'>('ALL');
  const [showCli, setShowCli] = useState(true);

  const filteredLogs = logs.filter(l => filterLevel === 'ALL' || l.level === filterLevel);

  return (
    <div className="flex h-[calc(100vh-180px)] space-x-8 pb-6 animate-fade-in">
      
      {/* Main Workflow & Logs */}
      <div className="flex-1 flex flex-col space-y-8 overflow-y-auto pr-2 custom-scrollbar">
        
        {/* Visual Pipeline Hub */}
        <div className="bg-white/70 dark:bg-nexus-800/50 backdrop-blur-xl border border-slate-200 dark:border-nexus-700/50 rounded-3xl p-8 shadow-sm relative overflow-hidden group">
          <div className="absolute top-0 right-0 w-64 h-64 bg-nexus-accent/5 rounded-full blur-3xl -mr-32 -mt-32"></div>
          
          <div className="flex justify-between items-center mb-10 relative z-10">
            <div>
              <h2 className="text-xl font-black text-slate-900 dark:text-white flex items-center tracking-tight">
                <Sliders className="mr-3 text-nexus-accent" size={20} />
                {t.mlops} <span className="text-slate-300 dark:text-nexus-700 mx-2 font-light">/</span> <span className="text-slate-400 text-sm font-bold uppercase tracking-widest">Pipeline v2.1</span>
              </h2>
              <p className="text-[10px] text-slate-400 dark:text-nexus-500 font-bold uppercase mt-1 tracking-widest">Autonomous Model Orchestration</p>
            </div>
            <div className="flex space-x-3">
              <button className="flex items-center px-4 py-2 bg-slate-100 dark:bg-nexus-900/50 text-slate-600 dark:text-nexus-300 rounded-xl hover:bg-slate-200 dark:hover:bg-nexus-700 transition-all text-[10px] font-black uppercase tracking-widest border border-slate-200 dark:border-nexus-700">
                <Zap size={14} className="mr-2 text-yellow-500" />
                {t.injectParams}
              </button>
              <button className="flex items-center px-5 py-2.5 bg-nexus-accent text-white rounded-xl hover:bg-blue-600 transition-all shadow-xl shadow-nexus-accent/25 text-[10px] font-black uppercase tracking-[0.2em]">
                <Play size={14} className="mr-2" />
                {t.executePipeline}
              </button>
            </div>
          </div>

          {/* Pipeline Canvas */}
          <div className="relative h-64 w-full bg-slate-50/50 dark:bg-nexus-900/30 rounded-2xl border border-dashed border-slate-200 dark:border-nexus-700 flex items-center justify-center overflow-x-auto shadow-inner group/canvas">
             <div className="flex items-center min-w-[900px] px-20">
                {MOCK_FLOW_NODES.map((node, idx) => (
                  <React.Fragment key={node.id}>
                    {idx > 0 && (
                      <div className="h-0.5 w-20 bg-slate-200 dark:bg-nexus-700 relative mx-2">
                         {node.status === 'running' && (
                           <div className="absolute top-1/2 left-0 w-full h-1.5 bg-nexus-accent -translate-y-1/2 animate-pulse rounded-full shadow-[0_0_10px_#3b82f6]"></div>
                         )}
                         {node.status === 'completed' && (
                           <div className="absolute top-1/2 left-0 w-full h-0.5 bg-green-500 -translate-y-1/2"></div>
                         )}
                      </div>
                    )}
                    
                    <div className="relative flex flex-col items-center group/node">
                       <div className={`w-36 h-20 rounded-2xl border-2 flex flex-col items-center justify-center bg-white dark:bg-nexus-800 shadow-xl transition-all duration-300 hover:scale-110 z-10 ${
                         node.status === 'completed' ? 'border-green-500/30' :
                         node.status === 'running' ? 'border-nexus-accent shadow-[0_0_30px_rgba(59,130,246,0.3)]' :
                         node.status === 'error' ? 'border-red-500/30' :
                         'border-slate-100 dark:border-nexus-700'
                       }`}>
                          <div className={`text-[9px] font-black uppercase tracking-widest mb-1 ${
                            node.status === 'running' ? 'text-nexus-accent' : 'text-slate-400 dark:text-nexus-500'
                          }`}>{node.type}</div>
                          <span className="font-bold text-xs text-slate-800 dark:text-white text-center px-2">{node.label}</span>
                       </div>
                       
                       <div className="absolute -top-3 -right-3 bg-white dark:bg-nexus-900 rounded-full p-1 shadow-lg border border-slate-100 dark:border-nexus-700 z-20">
                          {node.status === 'completed' && <CheckCircle size={18} className="text-green-500" />}
                          {node.status === 'running' && <Clock size={18} className="text-nexus-accent animate-spin" />}
                          {node.status === 'error' && <AlertTriangle size={18} className="text-red-500" />}
                          {node.status === 'pending' && <div className="w-[18px] h-[18px] rounded-full bg-slate-100 dark:bg-nexus-800"></div>}
                       </div>
                    </div>
                  </React.Fragment>
                ))}
             </div>
          </div>
          
          {/* Node Performance Indicators */}
          <div className="mt-8 grid grid-cols-4 gap-6 relative z-10">
             <div className="p-4 bg-white dark:bg-nexus-900/40 rounded-2xl border border-slate-100 dark:border-nexus-700/50 shadow-sm">
                <div className="flex items-center text-[9px] font-black uppercase tracking-widest text-slate-400 dark:text-nexus-500 mb-2">
                   <Clock size={12} className="mr-2" /> Execution Time
                </div>
                <div className="text-xl font-mono font-black text-slate-900 dark:text-white">04:21<span className="text-xs text-nexus-500 ml-1">sec</span></div>
             </div>
             <div className="p-4 bg-white dark:bg-nexus-900/40 rounded-2xl border border-slate-100 dark:border-nexus-700/50 shadow-sm col-span-2">
                 <div className="flex justify-between items-center mb-2">
                    <div className="flex items-center text-[9px] font-black uppercase tracking-widest text-slate-400 dark:text-nexus-500">
                       <Cpu size={12} className="mr-2" /> Parallel GPU Compute
                    </div>
                    <span className="text-[10px] font-bold text-green-500">Active (84%)</span>
                 </div>
                 <div className="w-full bg-slate-100 dark:bg-nexus-800 h-2 rounded-full overflow-hidden">
                    <div className="bg-gradient-to-r from-nexus-accent to-purple-500 h-full rounded-full shadow-[0_0_8px_rgba(59,130,246,0.5)] transition-all duration-1000" style={{ width: '84%' }}></div>
                 </div>
             </div>
             <div className="p-4 bg-white dark:bg-nexus-900/40 rounded-2xl border border-slate-100 dark:border-nexus-700/50 shadow-sm">
                <div className="flex items-center text-[9px] font-black uppercase tracking-widest text-slate-400 dark:text-nexus-500 mb-2">
                   <Zap size={12} className="mr-2 text-yellow-500" /> Token/Sec
                </div>
                <div className="text-xl font-mono font-black text-slate-900 dark:text-white">12.4k</div>
             </div>
          </div>
        </div>

        {/* Console & Log Panel */}
        <div className="flex-1 bg-white/70 dark:bg-nexus-800/50 backdrop-blur-xl border border-slate-200 dark:border-nexus-700/50 rounded-3xl shadow-sm flex flex-col min-h-[400px] overflow-hidden">
           <div className="p-6 border-b border-slate-100 dark:border-nexus-700 flex justify-between items-center bg-slate-50/50 dark:bg-nexus-900/30">
              <div className="flex items-center space-x-6">
                 <div>
                   <h3 className="text-xs font-black uppercase tracking-[0.2em] text-slate-900 dark:text-white flex items-center">
                     <TerminalIcon size={16} className="mr-3 text-nexus-accent" /> 
                     System Output Stream
                   </h3>
                 </div>
                 <div className="flex p-1 bg-slate-100 dark:bg-nexus-900 rounded-xl space-x-1">
                    <button 
                      onClick={() => setFilterLevel('ALL')}
                      className={`px-4 py-1.5 text-[9px] font-black uppercase tracking-widest rounded-lg transition-all ${filterLevel === 'ALL' ? 'bg-white dark:bg-nexus-800 text-nexus-accent shadow-sm' : 'text-slate-400 dark:text-nexus-500'}`}
                    >All Trace</button>
                    <button 
                      onClick={() => setFilterLevel('ERROR')}
                      className={`px-4 py-1.5 text-[9px] font-black uppercase tracking-widest rounded-lg transition-all ${filterLevel === 'ERROR' ? 'bg-red-500 text-white shadow-lg shadow-red-500/20' : 'text-slate-400 dark:text-nexus-500'}`}
                    >Fatal Errors</button>
                 </div>
              </div>
              <button className="flex items-center text-[10px] font-black uppercase tracking-widest text-nexus-accent hover:text-blue-400 bg-nexus-accent/5 px-3 py-1.5 rounded-lg border border-nexus-accent/20">
                 <Download size={14} className="mr-2" /> {t.export} Audit Log
              </button>
           </div>
           
           <div className="flex-1 overflow-y-auto font-mono text-xs custom-scrollbar">
              <table className="w-full text-left border-collapse">
                 <thead className="bg-slate-50/80 dark:bg-nexus-900/80 text-[10px] font-black uppercase tracking-widest text-slate-400 dark:text-nexus-500 sticky top-0 backdrop-blur z-10 border-b border-slate-100 dark:border-nexus-700">
                    <tr>
                       <th className="p-4 w-48">Timestamp</th>
                       <th className="p-4 w-24">Severity</th>
                       <th className="p-4 w-32">Namespace</th>
                       <th className="p-4">Message Context</th>
                    </tr>
                 </thead>
                 <tbody className="divide-y divide-slate-100 dark:divide-nexus-700/50">
                    {filteredLogs.map(log => (
                      <tr key={log.id} className="hover:bg-slate-50 dark:hover:bg-nexus-700/20 transition-all group">
                         <td className="p-4 text-slate-400 dark:text-nexus-500 font-mono tracking-tighter">{log.timestamp}</td>
                         <td className="p-4">
                            <span className={`px-2 py-0.5 rounded-lg text-[8px] font-black uppercase tracking-widest ${
                              log.level === 'INFO' ? 'bg-blue-500/10 text-blue-500 border border-blue-500/20' :
                              log.level === 'WARN' ? 'bg-yellow-500/10 text-yellow-600 border border-yellow-500/20' :
                              'bg-red-500/10 text-red-500 border border-red-500/20'
                            }`}>{log.level}</span>
                         </td>
                         <td className="p-4 text-slate-700 dark:text-nexus-200 font-bold tracking-tight uppercase text-[10px]">{log.stage}</td>
                         <td className={`p-4 ${log.level === 'ERROR' ? 'text-red-500' : 'text-slate-600 dark:text-nexus-300'} group-hover:text-nexus-accent transition-colors`}>
                           {log.message}
                         </td>
                      </tr>
                    ))}
                 </tbody>
              </table>
           </div>
        </div>
      </div>

      {/* Control Side Panel */}
      {showCli && (
        <div className="w-96 bg-nexus-900 border border-nexus-800 rounded-[2.5rem] shadow-2xl flex flex-col overflow-hidden animate-fade-in ring-1 ring-white/10">
           <div className="p-6 bg-nexus-800/50 border-b border-nexus-800 flex justify-between items-center">
              <div className="flex items-center">
                <div className="w-2 h-2 bg-nexus-accent rounded-full animate-pulse mr-3 shadow-[0_0_8px_#3b82f6]"></div>
                <span className="text-[10px] font-black uppercase tracking-[0.2em] text-white">Manual Overrides</span>
              </div>
              <button onClick={() => setShowCli(false)} className="p-2 hover:bg-nexus-700 rounded-xl transition-colors text-nexus-500"><ChevronRight size={18} /></button>
           </div>
           <div className="flex-1 p-8 font-mono text-[11px] text-nexus-400 space-y-8 overflow-y-auto custom-scrollbar">
              <div className="group">
                <div className="flex items-center text-nexus-600 mb-2 font-bold uppercase tracking-widest text-[9px]">
                  <Activity size={12} className="mr-2" /> Pipeline Run
                </div>
                <div className="bg-black/40 p-4 rounded-2xl border border-nexus-800 group-hover:border-nexus-accent/50 transition-all select-all cursor-pointer shadow-inner">
                  <span className="text-nexus-accent mr-2">$</span> mlflow run . --entry-point main --config-file experimental_v3.yaml
                </div>
              </div>
              <div className="group">
                <div className="flex items-center text-nexus-600 mb-2 font-bold uppercase tracking-widest text-[9px]">
                  <Search size={12} className="mr-2" /> Tail Logs
                </div>
                <div className="bg-black/40 p-4 rounded-2xl border border-nexus-800 group-hover:border-nexus-accent/50 transition-all select-all cursor-pointer shadow-inner">
                  <span className="text-nexus-accent mr-2">$</span> kubectl logs -f deploy/agent-pipeline-alpha --since=10m
                </div>
              </div>
              <div className="group">
                <div className="flex items-center text-nexus-600 mb-2 font-bold uppercase tracking-widest text-[9px]">
                  <Cpu size={12} className="mr-2" /> Resource Sync
                </div>
                <div className="bg-black/40 p-4 rounded-2xl border border-nexus-800 group-hover:border-nexus-accent/50 transition-all select-all cursor-pointer shadow-inner">
                  <span className="text-nexus-accent mr-2">$</span> terraform apply -var-file="gpu_cluster.tfvars"
                </div>
              </div>

              {/* Status Graphic */}
              <div className="pt-8 border-t border-nexus-800 mt-8">
                 <div className="flex justify-between items-end mb-4">
                    <div className="text-[10px] font-black uppercase text-nexus-500">Live Traffic Load</div>
                    <div className="text-xs font-mono text-white">1,402 req/s</div>
                 </div>
                 <div className="flex h-12 items-end space-x-1">
                    {[3, 6, 4, 8, 2, 7, 9, 4, 6, 8, 3, 5, 2, 9, 4, 6].map((h, i) => (
                      <div key={i} className="flex-1 bg-nexus-accent/20 rounded-t-sm relative overflow-hidden group/bar">
                         <div className="absolute bottom-0 left-0 right-0 bg-nexus-accent rounded-t-sm transition-all duration-500 group-hover/bar:bg-white" style={{ height: `${h * 10}%` }}></div>
                      </div>
                    ))}
                 </div>
              </div>
           </div>
        </div>
      )}
      {!showCli && (
         <button onClick={() => setShowCli(true)} className="w-12 bg-nexus-900 border-l border-nexus-800 flex items-center justify-center hover:bg-nexus-800 transition-all group">
            <TerminalIcon size={20} className="text-nexus-500 group-hover:text-nexus-accent transition-colors" />
         </button>
      )}
    </div>
  );
};

export default MLOps;
