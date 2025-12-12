import React, { useState } from 'react';
import { 
  Play, Download, Sliders, CheckCircle, Clock, AlertTriangle, 
  Terminal as TerminalIcon, Filter, Search, ChevronDown, ChevronRight 
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

  // Filter logs logic
  const filteredLogs = logs.filter(l => filterLevel === 'ALL' || l.level === filterLevel);

  return (
    <div className="flex h-[calc(100vh-140px)] space-x-6 pb-6">
      
      {/* Main Workflow Area */}
      <div className="flex-1 flex flex-col space-y-6 overflow-y-auto pr-2">
        
        {/* Visual Pipeline Builder */}
        <div className="bg-light-surface dark:bg-nexus-800 border border-light-border dark:border-nexus-700 rounded-lg p-6 shadow-sm relative overflow-hidden">
          <div className="flex justify-between items-center mb-8">
            <h2 className="text-xl font-bold text-light-text dark:text-white flex items-center">
              <Sliders className="mr-2 text-nexus-accent" />
              {t.mlops}
            </h2>
            <div className="flex space-x-3">
              <button className="flex items-center px-3 py-1.5 bg-nexus-100 dark:bg-nexus-700 text-nexus-600 dark:text-nexus-300 rounded hover:bg-nexus-200 dark:hover:bg-nexus-600 transition-colors text-sm font-medium">
                <Sliders size={14} className="mr-2" />
                {t.injectParams}
              </button>
              <button className="flex items-center px-3 py-1.5 bg-nexus-accent text-white rounded hover:bg-blue-600 transition-colors shadow-lg shadow-blue-500/20 text-sm font-medium">
                <Play size={14} className="mr-2" />
                {t.executePipeline}
              </button>
            </div>
          </div>

          {/* Simulated React Flow Chart */}
          <div className="relative h-64 w-full bg-nexus-50 dark:bg-nexus-900/50 rounded border border-dashed border-light-border dark:border-nexus-700 flex items-center justify-center overflow-x-auto">
             <div className="flex items-center min-w-[800px] px-10">
                {MOCK_FLOW_NODES.map((node, idx) => (
                  <React.Fragment key={node.id}>
                    {/* Connection Line */}
                    {idx > 0 && (
                      <div className="h-0.5 w-16 bg-nexus-300 dark:bg-nexus-600 relative mx-2">
                         {node.status === 'running' && (
                           <div className="absolute top-1/2 left-0 w-full h-1 bg-nexus-accent -translate-y-1/2 animate-pulse"></div>
                         )}
                      </div>
                    )}
                    
                    {/* Node */}
                    <div className={`relative flex flex-col items-center group cursor-pointer`}>
                       <div className={`w-32 h-16 rounded-lg border-2 flex items-center justify-center bg-light-surface dark:bg-nexus-800 shadow-md transition-all hover:scale-105 z-10 ${
                         node.status === 'completed' ? 'border-nexus-success' :
                         node.status === 'running' ? 'border-nexus-accent shadow-[0_0_15px_rgba(59,130,246,0.4)]' :
                         node.status === 'error' ? 'border-nexus-danger' :
                         'border-light-border dark:border-nexus-600'
                       }`}>
                          <span className={`font-semibold text-sm ${
                            node.status === 'running' ? 'text-nexus-accent' : 'text-light-text dark:text-white'
                          }`}>{node.label}</span>
                       </div>
                       
                       {/* Status Icon */}
                       <div className="absolute -top-3 -right-2 bg-light-surface dark:bg-nexus-800 rounded-full p-0.5 shadow-sm">
                          {node.status === 'completed' && <CheckCircle size={16} className="text-nexus-success" />}
                          {node.status === 'running' && <Clock size={16} className="text-nexus-accent animate-spin" />}
                          {node.status === 'error' && <AlertTriangle size={16} className="text-nexus-danger" />}
                       </div>
                    </div>
                  </React.Fragment>
                ))}
             </div>
          </div>
          
          {/* Real-time Metrics Overlay */}
          <div className="mt-6 grid grid-cols-3 gap-4">
             <div className="p-3 bg-nexus-50 dark:bg-nexus-900/30 rounded border border-light-border dark:border-nexus-700">
                <div className="text-xs text-light-textSec dark:text-nexus-400 mb-1">Execution Time</div>
                <div className="text-xl font-mono text-light-text dark:text-white">04:21<span className="text-sm text-nexus-400">s</span></div>
             </div>
             <div className="p-3 bg-nexus-50 dark:bg-nexus-900/30 rounded border border-light-border dark:border-nexus-700">
                 <div className="text-xs text-light-textSec dark:text-nexus-400 mb-1">GPU Util</div>
                 <div className="w-full bg-nexus-200 dark:bg-nexus-700 h-2 rounded-full mt-2">
                    <div className="bg-nexus-success h-2 rounded-full" style={{ width: '84%' }}></div>
                 </div>
             </div>
             <div className="p-3 bg-nexus-50 dark:bg-nexus-900/30 rounded border border-light-border dark:border-nexus-700">
                <div className="text-xs text-light-textSec dark:text-nexus-400 mb-1">Cost Est.</div>
                <div className="text-xl font-mono text-light-text dark:text-white">$0.42</div>
             </div>
          </div>
        </div>

        {/* Log Retention Panel */}
        <div className="flex-1 bg-light-surface dark:bg-nexus-800 border border-light-border dark:border-nexus-700 rounded-lg shadow-sm flex flex-col min-h-[300px]">
           <div className="p-4 border-b border-light-border dark:border-nexus-700 flex justify-between items-center bg-nexus-50/50 dark:bg-nexus-800/50">
              <div className="flex items-center space-x-4">
                 <h3 className="font-bold text-light-text dark:text-white flex items-center">
                   <TerminalIcon size={16} className="mr-2 text-nexus-500" /> 
                   {t.logs}
                 </h3>
                 <div className="flex space-x-2">
                    <button 
                      onClick={() => setFilterLevel('ALL')}
                      className={`px-2 py-1 text-xs rounded border ${filterLevel === 'ALL' ? 'bg-nexus-200 dark:bg-nexus-600 text-light-text dark:text-white border-transparent' : 'border-light-border dark:border-nexus-600 text-light-textSec dark:text-nexus-400'}`}
                    >All</button>
                    <button 
                      onClick={() => setFilterLevel('ERROR')}
                      className={`px-2 py-1 text-xs rounded border ${filterLevel === 'ERROR' ? 'bg-red-500/20 text-red-500 border-red-500/30' : 'border-light-border dark:border-nexus-600 text-light-textSec dark:text-nexus-400'}`}
                    >Errors</button>
                 </div>
              </div>
              <button className="flex items-center text-xs text-nexus-accent hover:text-blue-400">
                 <Download size={14} className="mr-1" /> {t.export} JSON
              </button>
           </div>
           
           <div className="flex-1 overflow-y-auto p-0 font-mono text-sm">
              <table className="w-full text-left border-collapse">
                 <thead className="bg-nexus-50 dark:bg-nexus-900/50 text-xs text-light-textSec dark:text-nexus-400 sticky top-0">
                    <tr>
                       <th className="p-3 w-40">Timestamp</th>
                       <th className="p-3 w-20">Level</th>
                       <th className="p-3 w-32">Stage</th>
                       <th className="p-3">Message</th>
                    </tr>
                 </thead>
                 <tbody className="divide-y divide-light-border dark:divide-nexus-700">
                    {filteredLogs.map(log => (
                      <tr key={log.id} className="hover:bg-nexus-50 dark:hover:bg-nexus-700/30 transition-colors">
                         <td className="p-3 text-light-textSec dark:text-nexus-500 whitespace-nowrap">{log.timestamp}</td>
                         <td className="p-3">
                            <span className={`px-1.5 py-0.5 rounded text-[10px] font-bold ${
                              log.level === 'INFO' ? 'bg-blue-500/10 text-blue-500' :
                              log.level === 'WARN' ? 'bg-yellow-500/10 text-yellow-500' :
                              'bg-red-500/10 text-red-500'
                            }`}>{log.level}</span>
                         </td>
                         <td className="p-3 text-light-text dark:text-nexus-300">{log.stage}</td>
                         <td className={`p-3 ${log.level === 'ERROR' ? 'text-red-500' : 'text-light-textSec dark:text-nexus-300'}`}>
                           {log.message}
                         </td>
                      </tr>
                    ))}
                 </tbody>
              </table>
           </div>
        </div>
      </div>

      {/* CLI Integration Side Panel */}
      {showCli && (
        <div className="w-80 bg-nexus-900 border border-nexus-700 rounded-lg shadow-xl flex flex-col overflow-hidden">
           <div className="p-3 bg-nexus-800 border-b border-nexus-700 flex justify-between items-center">
              <span className="text-xs font-mono font-bold text-nexus-300">CLI Integration</span>
              <button onClick={() => setShowCli(false)}><ChevronRight size={16} className="text-nexus-500" /></button>
           </div>
           <div className="flex-1 p-4 font-mono text-xs text-nexus-300 space-y-4 overflow-y-auto">
              <div>
                <div className="text-nexus-500 mb-1"># Execute Pipeline</div>
                <div className="bg-black p-2 rounded border border-nexus-700 select-all cursor-pointer hover:border-nexus-500 transition-colors">
                  mlflow run . --entry-point main
                </div>
              </div>
              <div>
                <div className="text-nexus-500 mb-1"># Check Training Logs</div>
                <div className="bg-black p-2 rounded border border-nexus-700 select-all cursor-pointer hover:border-nexus-500 transition-colors">
                  tail -f /logs/train_job_32.log
                </div>
              </div>
              <div>
                <div className="text-nexus-500 mb-1"># Deploy to K8s</div>
                <div className="bg-black p-2 rounded border border-nexus-700 select-all cursor-pointer hover:border-nexus-500 transition-colors">
                  kubectl apply -f deployment.yaml
                </div>
              </div>
           </div>
        </div>
      )}
      {!showCli && (
         <button onClick={() => setShowCli(true)} className="w-8 bg-nexus-800 border border-nexus-700 rounded-l-lg flex items-center justify-center hover:bg-nexus-700 transition-colors">
            <TerminalIcon size={16} className="text-nexus-400 rotate-90" />
         </button>
      )}
    </div>
  );
};

export default MLOps;