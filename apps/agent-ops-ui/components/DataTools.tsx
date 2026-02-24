
import React, { useState } from 'react';
import { 
  Database, Server, Search, Activity, Power, RefreshCw, X, ArrowRight 
} from 'lucide-react';
import { 
  LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, PieChart, Pie, Cell 
} from 'recharts';
import { MOCK_DATA_NODES, TRANSLATIONS } from '../constants';
import { Language, DataToolNode } from '../types';

interface DataToolsProps {
  lang: Language;
}

const DataTools: React.FC<DataToolsProps> = ({ lang }) => {
  const t = TRANSLATIONS[lang];
  const [selectedTool, setSelectedTool] = useState<DataToolNode | null>(null);
  const [activeTab, setActiveTab] = useState<'overview' | 'logs' | 'cli'>('overview');
  
  const currentYear = new Date().getFullYear();

  const ToolIcon = ({ type }: { type: string }) => {
    switch (type) {
      case 'Elasticsearch': return <Search className="text-blue-500" />;
      case 'Redis': return <Database className="text-red-500" />;
      default: return <Server className="text-purple-500" />;
    }
  };

  const mockChartData = Array.from({ length: 10 }).map((_, i) => ({
    time: `10:0${i}`,
    qps: Math.floor(Math.random() * 500) + 1000,
    latency: Math.floor(Math.random() * 20) + 5
  }));

  const pieData = [
    { name: 'Used', value: selectedTool ? selectedTool.diskUsage : 0 },
    { name: 'Free', value: 100 - (selectedTool ? selectedTool.diskUsage : 0) },
  ];

  return (
    <div className="h-full pb-20 relative">
      
      {/* Topology Header Visualization */}
      <div className="mb-8 p-6 bg-light-surface dark:bg-nexus-800 border border-light-border dark:border-nexus-700 rounded-lg shadow-sm">
         <h2 className="text-lg font-bold text-light-text dark:text-white mb-6 flex items-center">
            <Activity className="mr-2 text-nexus-accent" /> {t.dataTools} {t.topology}
         </h2>
         <div className="flex items-center justify-center space-x-8 overflow-x-auto py-4">
            {MOCK_DATA_NODES.map((node, index) => (
               <React.Fragment key={node.id}>
                  <div 
                    onClick={() => setSelectedTool(node)}
                    className="flex flex-col items-center cursor-pointer group"
                  >
                     <div className={`w-16 h-16 rounded-xl flex items-center justify-center border-2 transition-all shadow-md ${
                        selectedTool?.id === node.id ? 'border-nexus-accent bg-nexus-100 dark:bg-nexus-700' : 'border-light-border dark:border-nexus-600 bg-light-surface dark:bg-nexus-900 hover:border-nexus-400'
                     }`}>
                        <ToolIcon type={node.type} />
                     </div>
                     <span className="mt-2 text-xs font-bold text-light-text dark:text-white">{node.name}</span>
                     <span className="text-[10px] text-light-textSec dark:text-nexus-400">{node.type}</span>
                     
                     {/* Status Dot */}
                     <div className={`mt-1 w-2 h-2 rounded-full ${
                        node.status === 'green' ? 'bg-green-500' : 'bg-yellow-500'
                     }`}></div>
                  </div>
                  
                  {/* Connection Arrow */}
                  {node.connections.length > 0 && (
                     <div className="flex items-center text-nexus-300 dark:text-nexus-600">
                        <div className="h-0.5 w-12 bg-current"></div>
                        <ArrowRight size={16} className="-ml-2" />
                     </div>
                  )}
               </React.Fragment>
            ))}
         </div>
      </div>

      {/* Grid View */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        {MOCK_DATA_NODES.map(node => (
          <div 
            key={node.id} 
            onClick={() => setSelectedTool(node)}
            className="bg-light-surface dark:bg-nexus-800 border border-light-border dark:border-nexus-700 p-5 rounded-lg shadow-sm hover:shadow-lg transition-all cursor-pointer group"
          >
            <div className="flex justify-between items-start mb-4">
              <div className="p-3 rounded-lg bg-nexus-50 dark:bg-nexus-900 group-hover:bg-nexus-200 dark:group-hover:bg-nexus-700 transition-colors">
                <ToolIcon type={node.type} />
              </div>
              <span className={`px-2 py-0.5 rounded text-[10px] font-bold uppercase ${
                node.status === 'green' ? 'bg-green-500/10 text-green-500' : 
                node.status === 'yellow' ? 'bg-yellow-500/10 text-yellow-500' : 'bg-red-500/10 text-red-500'
              }`}>{node.status}</span>
            </div>
            <h3 className="text-lg font-bold text-light-text dark:text-white mb-1">{node.name}</h3>
            <div className="space-y-2 text-sm text-light-textSec dark:text-nexus-300 mt-4">
               <div className="flex justify-between"><span>Shards:</span> <span className="text-light-text dark:text-white font-mono">{node.shards}</span></div>
               <div className="flex justify-between"><span>Disk:</span> <span className="text-light-text dark:text-white font-mono">{node.diskUsage}%</span></div>
               <div className="flex justify-between"><span>QPS:</span> <span className="text-light-text dark:text-white font-mono">{node.qps}</span></div>
            </div>
          </div>
        ))}
      </div>

      {/* Detailed Modal */}
      {selectedTool && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm p-4 animate-fade-in">
           <div className="bg-light-surface dark:bg-nexus-800 border border-light-border dark:border-nexus-700 rounded-xl shadow-2xl w-full max-w-4xl max-h-[90vh] flex flex-col overflow-hidden">
              
              {/* Modal Header */}
              <div className="p-6 border-b border-light-border dark:border-nexus-700 flex justify-between items-center bg-nexus-50/50 dark:bg-nexus-900/50">
                 <div className="flex items-center space-x-4">
                    <div className="p-3 bg-white dark:bg-nexus-700 rounded-lg shadow-sm border border-light-border dark:border-nexus-600">
                       <ToolIcon type={selectedTool.type} />
                    </div>
                    <div>
                       <h2 className="text-xl font-bold text-light-text dark:text-white">{selectedTool.name}</h2>
                       <p className="text-sm text-light-textSec dark:text-nexus-400">ID: {selectedTool.id} • {selectedTool.type} Node</p>
                    </div>
                 </div>
                 <div className="flex items-center space-x-2">
                    <button className="px-3 py-1.5 bg-red-500/10 text-red-500 hover:bg-red-500 hover:text-white rounded transition-colors text-sm font-bold flex items-center">
                       <Power size={14} className="mr-2" /> {t.restartNode}
                    </button>
                    <button onClick={() => setSelectedTool(null)} className="p-2 hover:bg-nexus-200 dark:hover:bg-nexus-700 rounded-full transition-colors text-nexus-500">
                       <X size={20} />
                    </button>
                 </div>
              </div>

              {/* Modal Tabs */}
              <div className="flex border-b border-light-border dark:border-nexus-700 bg-light-surface dark:bg-nexus-800">
                 {['overview', 'logs', 'cli'].map(tab => (
                    <button
                      key={tab}
                      onClick={() => setActiveTab(tab as any)}
                      className={`px-6 py-3 text-sm font-bold border-b-2 transition-colors capitalize ${
                        activeTab === tab 
                        ? 'border-nexus-accent text-nexus-accent bg-nexus-50 dark:bg-nexus-700/30' 
                        : 'border-transparent text-light-textSec dark:text-nexus-400 hover:text-light-text dark:hover:text-white'
                      }`}
                    >
                       {tab}
                    </button>
                 ))}
              </div>

              {/* Modal Content */}
              <div className="flex-1 overflow-y-auto p-6 bg-light-bg dark:bg-nexus-900/30">
                 {activeTab === 'overview' && (
                    <div className="grid grid-cols-3 gap-6">
                       <div className="col-span-2 bg-light-surface dark:bg-nexus-800 p-4 rounded-lg border border-light-border dark:border-nexus-700 shadow-sm">
                          <h4 className="font-bold text-light-text dark:text-white mb-4">Throughput (QPS)</h4>
                          <div className="h-64">
                             <ResponsiveContainer width="100%" height="100%">
                                <LineChart data={mockChartData}>
                                   <CartesianGrid strokeDasharray="3 3" stroke="#64748b" opacity={0.2} vertical={false} />
                                   <XAxis dataKey="time" stroke="#64748b" fontSize={10} />
                                   <YAxis stroke="#64748b" fontSize={10} />
                                   <Tooltip contentStyle={{ borderRadius: '8px', border: 'none' }} />
                                   <Line type="monotone" dataKey="qps" stroke="#3b82f6" strokeWidth={2} dot={false} />
                                </LineChart>
                             </ResponsiveContainer>
                          </div>
                       </div>
                       <div className="bg-light-surface dark:bg-nexus-800 p-4 rounded-lg border border-light-border dark:border-nexus-700 shadow-sm flex flex-col items-center justify-center">
                          <h4 className="font-bold text-light-text dark:text-white mb-4">Storage Usage</h4>
                          <div className="h-40 w-full relative">
                             <ResponsiveContainer width="100%" height="100%">
                                <PieChart>
                                   <Pie data={pieData} innerRadius={40} outerRadius={60} dataKey="value">
                                      <Cell fill="#ef4444" />
                                      <Cell fill="#334155" />
                                   </Pie>
                                </PieChart>
                             </ResponsiveContainer>
                             <div className="absolute inset-0 flex items-center justify-center font-bold text-lg text-light-text dark:text-white">
                                {selectedTool.diskUsage}%
                             </div>
                          </div>
                          <div className="text-xs text-light-textSec dark:text-nexus-400 mt-2">1.2TB / 4TB</div>
                       </div>
                    </div>
                 )}

                 {activeTab === 'logs' && (
                    <div className="bg-nexus-900 rounded-lg p-4 font-mono text-xs text-nexus-300 h-96 overflow-y-auto border border-nexus-700 shadow-inner">
                       <div className="text-nexus-500 mb-2 border-b border-nexus-700 pb-2">/var/log/{selectedTool.type.toLowerCase()}/error.log</div>
                       <div className="space-y-1">
                          <div>[{currentYear}-10-28 14:20:01] INFO: Node started successfully.</div>
                          <div>[{currentYear}-10-28 14:20:05] INFO: Cluster health changed from RED to YELLOW.</div>
                          <div className="text-yellow-400">[{currentYear}-10-28 14:21:00] WARN: High memory pressure detected (85%).</div>
                          <div className="text-green-400">[{currentYear}-10-28 14:22:30] INFO: Garbage collection completed in 240ms.</div>
                          <div>[{currentYear}-10-28 14:25:00] INFO: Indexing batch #40291 complete.</div>
                       </div>
                    </div>
                 )}

                 {activeTab === 'cli' && (
                    <div className="h-96 flex flex-col bg-nexus-900 rounded-lg border border-nexus-700 shadow-inner overflow-hidden">
                       <div className="bg-nexus-800 px-4 py-2 text-xs font-mono text-nexus-400 border-b border-nexus-700 flex justify-between">
                          <span>{selectedTool.type} CLI Interface</span>
                          <span className="text-green-400">Connected</span>
                       </div>
                       <div className="flex-1 p-4 font-mono text-sm text-white overflow-y-auto">
                          <div>Welcome to the {selectedTool.type} monitor.</div>
                          <div>Type 'help' for commands.</div>
                          <br/>
                          <div className="flex items-center">
                             <span className="text-green-400 mr-2">{selectedTool.type.toLowerCase()}&gt;</span>
                             <span className="animate-pulse">_</span>
                          </div>
                       </div>
                    </div>
                 )}
              </div>
           </div>
        </div>
      )}

    </div>
  );
};

export default DataTools;
