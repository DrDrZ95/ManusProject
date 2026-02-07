
import React, { useState } from 'react';
import { 
  Upload, Database, Zap, Send, Check, Activity, Search, RefreshCw, Layers, BrainCircuit, ShieldCheck, TrendingUp
} from 'lucide-react';
import { 
  RadarChart, PolarGrid, PolarAngleAxis, PolarRadiusAxis, Radar, ResponsiveContainer,
  LineChart, Line, CartesianGrid, XAxis, YAxis, Tooltip
} from 'recharts';
import { MOCK_LOCAL_MODELS, TRANSLATIONS } from '../constants';
import { Language } from '../types';

interface LocalModelProps {
  lang: Language;
}

const LocalModel: React.FC<LocalModelProps> = ({ lang }) => {
  const t = TRANSLATIONS[lang];
  const [activeTab, setActiveTab] = useState<'fine-tune' | 'data' | 'deploy'>('fine-tune');
  const [deployingId, setDeployingId] = useState<string | null>(null);

  const handleDeploy = (id: string) => {
    setDeployingId(id);
    setTimeout(() => {
      setDeployingId(null);
      alert("AgentProject Deployment Successful! Service is live at api.agentproject.local:8080");
    }, 2000);
  };

  const lossData = Array.from({length: 15}).map((_, i) => ({
    epoch: i + 1,
    train: Math.exp(-i * 0.15) + 0.1,
    val: Math.exp(-i * 0.1) + 0.2
  }));

  const radarData = [
    { subject: 'Accuracy', A: 88, fullMark: 100 },
    { subject: 'Precision', A: 85, fullMark: 100 },
    { subject: 'Recall', A: 90, fullMark: 100 },
    { subject: 'F1 Score', A: 87, fullMark: 100 },
    { subject: 'Latency', A: 70, fullMark: 100 },
    { subject: 'Reliability', A: 95, fullMark: 100 },
  ];

  return (
    <div className="h-full flex flex-col pb-20 animate-fade-in">
      
      {/* Refined Navigation Tabs */}
      <div className="flex p-1.5 bg-slate-100 dark:bg-nexus-800/50 backdrop-blur rounded-2xl mb-10 w-fit border border-slate-200 dark:border-nexus-700/50 self-center">
        <button 
          onClick={() => setActiveTab('fine-tune')}
          className={`px-8 py-2.5 font-black text-[10px] uppercase tracking-[0.2em] rounded-xl transition-all flex items-center ${activeTab === 'fine-tune' ? 'bg-white dark:bg-nexus-800 text-nexus-accent shadow-lg' : 'text-slate-400 dark:text-nexus-500 hover:text-slate-900 dark:hover:text-white'}`}
        >
          <Zap size={14} className="mr-3" /> {t.fineTuning}
        </button>
        <button 
          onClick={() => setActiveTab('data')}
          className={`px-8 py-2.5 font-black text-[10px] uppercase tracking-[0.2em] rounded-xl transition-all flex items-center ${activeTab === 'data' ? 'bg-white dark:bg-nexus-800 text-nexus-accent shadow-lg' : 'text-slate-400 dark:text-nexus-500 hover:text-slate-900 dark:hover:text-white'}`}
        >
          <Database size={14} className="mr-3" /> {t.dataCollection}
        </button>
        <button 
          onClick={() => setActiveTab('deploy')}
          className={`px-8 py-2.5 font-black text-[10px] uppercase tracking-[0.2em] rounded-xl transition-all flex items-center ${activeTab === 'deploy' ? 'bg-white dark:bg-nexus-800 text-nexus-accent shadow-lg' : 'text-slate-400 dark:text-nexus-500 hover:text-slate-900 dark:hover:text-white'}`}
        >
          <Send size={14} className="mr-3" /> {t.deployment}
        </button>
      </div>

      {/* Content Area */}
      <div className="flex-1 overflow-y-auto custom-scrollbar">
        
        {/* Fine-Tuning View */}
        {activeTab === 'fine-tune' && (
          <div className="grid grid-cols-1 lg:grid-cols-12 gap-8 animate-fade-in">
             <div className="lg:col-span-8 bg-white/70 dark:bg-nexus-800/60 backdrop-blur-xl border border-slate-200 dark:border-nexus-700/50 rounded-[2.5rem] shadow-sm overflow-hidden flex flex-col">
                <div className="p-8 border-b border-slate-100 dark:border-nexus-700/50 flex justify-between items-center bg-slate-50/50 dark:bg-nexus-900/30">
                  <div>
                    <h3 className="text-xs font-black uppercase tracking-[0.2em] text-slate-900 dark:text-white flex items-center">
                      <BrainCircuit size={16} className="mr-3 text-nexus-accent" />
                      Active Model Training Tasks
                    </h3>
                  </div>
                  <button className="p-2.5 bg-slate-100 dark:bg-nexus-900 hover:bg-slate-200 dark:hover:bg-nexus-700 rounded-2xl transition-all text-slate-500 dark:text-nexus-400 shadow-sm"><RefreshCw size={18}/></button>
                </div>
                <div className="overflow-x-auto">
                  <table className="w-full text-left">
                    <thead className="bg-slate-50/50 dark:bg-nexus-900/50 text-[10px] font-black uppercase tracking-widest text-slate-400 dark:text-nexus-500 border-b border-slate-100 dark:border-nexus-700">
                        <tr>
                          <th className="p-6">Asset Designation</th>
                          <th className="p-6">Base Weights</th>
                          <th className="p-6">Optimization</th>
                          <th className="p-6">Status</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-slate-100 dark:divide-nexus-700/50">
                        {MOCK_LOCAL_MODELS.map(model => (
                          <tr key={model.id} className="hover:bg-slate-50 dark:hover:bg-nexus-700/20 transition-all group">
                            <td className="p-6">
                              <div className="font-bold text-slate-900 dark:text-white group-hover:text-nexus-accent transition-colors">{model.name}</div>
                              <div className="text-[9px] font-black uppercase tracking-widest text-slate-400 mt-1">ID: {model.id}</div>
                            </td>
                            <td className="p-6">
                              <span className="text-[10px] font-black text-slate-600 dark:text-nexus-400 font-mono bg-slate-100 dark:bg-nexus-900 px-2 py-1 rounded-lg border border-slate-200 dark:border-nexus-700 uppercase tracking-tight">{model.baseModel}</span>
                            </td>
                            <td className="p-6">
                              <div className="flex items-center space-x-3">
                                <div className="flex-1 min-w-[100px] bg-slate-100 dark:bg-nexus-900 h-2 rounded-full overflow-hidden shadow-inner">
                                  <div className={`h-full rounded-full transition-all duration-1000 ${model.status === 'Fine-Tuning' ? 'bg-nexus-accent animate-pulse' : 'bg-green-500'}`} style={{ width: `${(model.epoch / model.totalEpochs) * 100}%` }}></div>
                                </div>
                                <span className="text-[10px] font-mono font-black text-slate-500 dark:text-nexus-400">{model.epoch}/{model.totalEpochs}</span>
                              </div>
                            </td>
                            <td className="p-6">
                                <span className={`inline-flex items-center px-3 py-1 rounded-xl text-[9px] font-black uppercase tracking-widest border transition-all ${
                                  model.status === 'Fine-Tuning' ? 'bg-blue-500/10 text-blue-500 border-blue-500/20' :
                                  model.status === 'Deployed' ? 'bg-green-500 text-white shadow-lg shadow-green-500/20 border-transparent' :
                                  'bg-slate-500/10 text-slate-500 border-slate-500/20'
                                }`}>
                                  {model.status === 'Fine-Tuning' && <RefreshCw size={10} className="mr-2 animate-spin" />}
                                  {model.status === 'Deployed' && <ShieldCheck size={10} className="mr-2" />}
                                  {model.status}
                                </span>
                            </td>
                          </tr>
                        ))}
                    </tbody>
                  </table>
                </div>
             </div>

             <div className="lg:col-span-4 flex flex-col space-y-8">
                {/* Loss Chart */}
                <div className="bg-white/70 dark:bg-nexus-800/60 backdrop-blur-xl border border-slate-200 dark:border-nexus-700/50 rounded-[2.5rem] p-8 shadow-sm group">
                  <h3 className="text-xs font-black uppercase tracking-[0.2em] text-slate-900 dark:text-white mb-8 flex items-center">
                    <TrendingUp size={16} className="mr-3 text-red-500" />
                    Objective Loss History
                  </h3>
                  <div className="h-64 w-full">
                    <ResponsiveContainer width="100%" height="100%">
                        <LineChart data={lossData}>
                          <CartesianGrid strokeDasharray="5 5" stroke="#94a3b8" opacity={0.05} vertical={false} />
                          <XAxis dataKey="epoch" stroke="#64748b" fontSize={9} tickLine={false} axisLine={false} dy={10} />
                          <YAxis stroke="#64748b" fontSize={9} tickLine={false} axisLine={false} />
                          <Tooltip 
                            contentStyle={{ backgroundColor: '#0f172a', borderRadius: '16px', border: 'none', boxShadow: '0 25px 50px -12px rgba(0,0,0,0.5)', fontSize: '11px', color: '#fff', padding: '15px' }}
                          />
                          <Line type="monotone" dataKey="train" stroke="#3b82f6" strokeWidth={3} dot={false} animationDuration={3000} />
                          <Line type="monotone" dataKey="val" stroke="#ef4444" strokeWidth={3} dot={false} strokeDasharray="5 5" animationDuration={3000} />
                        </LineChart>
                    </ResponsiveContainer>
                  </div>
                  <div className="mt-6 flex justify-center space-x-6">
                    <div className="flex items-center text-[10px] font-black uppercase text-slate-400 dark:text-nexus-500">
                      <span className="w-2.5 h-2.5 rounded-full bg-nexus-accent mr-2"></span> Train Loss
                    </div>
                    <div className="flex items-center text-[10px] font-black uppercase text-slate-400 dark:text-nexus-500">
                      <span className="w-2.5 h-2.5 rounded-full bg-red-500 mr-2 border-2 border-dashed border-white"></span> Val Loss
                    </div>
                  </div>
                </div>

                {/* Compute Resource Quick Meter */}
                <div className="bg-nexus-900 rounded-[2rem] p-6 border border-nexus-800 flex flex-col justify-center">
                   <div className="flex justify-between items-end mb-4">
                      <div className="text-[10px] font-black uppercase text-nexus-500 tracking-[0.2em]">HBM Memory Usage</div>
                      <div className="text-xs font-mono text-white">64.2 GB / 80 GB</div>
                   </div>
                   <div className="w-full bg-nexus-800 h-3 rounded-full overflow-hidden shadow-inner">
                      <div className="h-full bg-gradient-to-r from-blue-500 to-purple-500 rounded-full shadow-[0_0_12px_rgba(168,85,247,0.5)]" style={{ width: '80%' }}></div>
                   </div>
                </div>
             </div>
          </div>
        )}

        {/* Data Collection View */}
        {activeTab === 'data' && (
          <div className="animate-fade-in space-y-8">
             <div className="bg-white/70 dark:bg-nexus-800/60 backdrop-blur-xl border border-slate-200 dark:border-nexus-700/50 rounded-[3rem] p-16 text-center shadow-sm group">
                <div className="max-w-xl mx-auto border-2 border-dashed border-slate-200 dark:border-nexus-700 rounded-[2.5rem] p-16 hover:border-nexus-accent hover:bg-nexus-accent/[0.02] transition-all cursor-pointer group/upload">
                   <div className="w-24 h-24 bg-nexus-accent/10 rounded-[2rem] flex items-center justify-center mx-auto mb-8 group-hover/upload:scale-110 group-hover/upload:bg-nexus-accent group-hover/upload:text-white transition-all shadow-inner">
                      <Upload size={48} className="text-nexus-accent group-hover/upload:text-white" />
                   </div>
                   <h3 className="text-2xl font-black text-slate-900 dark:text-white tracking-tight">AgentProject Intelligence Feed</h3>
                   <p className="text-slate-500 dark:text-nexus-400 mt-3 text-sm font-medium leading-relaxed">Drag & drop your datasets (CSV, JSONL, Parquet) to initialize the fine-tuning pipeline.</p>
                   <button className="mt-8 px-8 py-3 bg-nexus-accent text-white rounded-2xl font-black text-[10px] uppercase tracking-[0.2em] hover:bg-blue-600 transition-all shadow-xl shadow-nexus-accent/25 active:scale-95">Browse Intelligence Files</button>
                </div>
             </div>
             
             <div className="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-6 gap-6">
                {[1, 2, 3, 4, 5, 6].map(i => (
                  <div key={i} className="group bg-white/50 dark:bg-nexus-800/40 backdrop-blur-sm border border-slate-200/50 dark:border-nexus-700/50 rounded-3xl p-6 text-center shadow-sm hover:shadow-xl hover:-translate-y-1 transition-all cursor-pointer">
                     <div className="w-12 h-12 bg-slate-100 dark:bg-nexus-900 rounded-2xl flex items-center justify-center mx-auto mb-4 text-slate-400 group-hover:text-nexus-accent transition-colors">
                        <Database size={24} />
                     </div>
                     <div className="text-[10px] font-black text-slate-900 dark:text-white uppercase tracking-tight truncate">Dataset_Delta_{i}.parquet</div>
                     <div className="text-[8px] font-black text-slate-400 uppercase tracking-widest mt-1">4.2 GB</div>
                  </div>
                ))}
             </div>
          </div>
        )}

        {/* Deployment View */}
        {activeTab === 'deploy' && (
          <div className="grid grid-cols-1 lg:grid-cols-12 gap-8 animate-fade-in">
             <div className="lg:col-span-7 bg-white/70 dark:bg-nexus-800/60 backdrop-blur-xl border border-slate-200 dark:border-nexus-700/50 rounded-[2.5rem] p-10 shadow-sm">
                <h3 className="text-xs font-black uppercase tracking-[0.2em] text-slate-900 dark:text-white mb-10 flex items-center">
                   <Activity size={18} className="mr-3 text-nexus-accent" />
                   Model Performance Analytics
                </h3>
                <div className="h-[400px]">
                   <ResponsiveContainer width="100%" height="100%">
                      <RadarChart outerRadius={150} data={radarData}>
                         <PolarGrid stroke="#94a3b8" opacity={0.1} />
                         <PolarAngleAxis dataKey="subject" tick={{ fill: '#64748b', fontSize: 10, fontWeight: 'bold' }} />
                         <PolarRadiusAxis angle={30} domain={[0, 100]} stroke="transparent" />
                         <Radar name="LLaMA-3-8B" dataKey="A" stroke="#3b82f6" strokeWidth={3} fill="#3b82f6" fillOpacity={0.15} />
                         <Tooltip 
                            contentStyle={{ backgroundColor: '#0f172a', borderRadius: '12px', border: 'none', color: '#fff' }}
                         />
                      </RadarChart>
                   </ResponsiveContainer>
                </div>
             </div>

             <div className="lg:col-span-5 flex flex-col space-y-8">
                <div className="bg-white/70 dark:bg-nexus-800/60 backdrop-blur-xl border border-slate-200 dark:border-nexus-700/50 rounded-[2.5rem] p-10 shadow-sm flex flex-col">
                   <h3 className="text-xs font-black uppercase tracking-[0.2em] text-slate-900 dark:text-white mb-4">Autonomous Deployment Hub</h3>
                   <p className="text-[11px] font-medium text-slate-500 dark:text-nexus-400 mb-8 leading-relaxed">Publish weights to production-ready inference endpoints with automated load balancing and eBPF monitoring.</p>
                   
                   <div className="space-y-4 mb-10">
                      <div className="bg-slate-50/50 dark:bg-nexus-900/50 p-5 rounded-2xl border border-slate-100 dark:border-nexus-700/50 shadow-inner">
                         <label className="text-[9px] text-slate-400 dark:text-nexus-500 uppercase font-black tracking-[0.2em]">Target RPC Host</label>
                         <div className="text-xs font-mono font-bold text-slate-800 dark:text-white mt-2 truncate">grpc://mesh.agentproject.internal:50051</div>
                      </div>
                      <div className="bg-slate-50/50 dark:bg-nexus-900/50 p-5 rounded-2xl border border-slate-100 dark:border-nexus-700/50 shadow-inner">
                         <label className="text-[9px] text-slate-400 dark:text-nexus-500 uppercase font-black tracking-[0.2em]">Accelerator Clusters</label>
                         <div className="text-xs font-mono font-bold text-slate-800 dark:text-white mt-2">NVIDIA-H100-80GB (NODE_04, NODE_05)</div>
                      </div>
                   </div>

                   <button 
                     onClick={() => handleDeploy('m1')}
                     disabled={!!deployingId}
                     className={`w-full py-4 rounded-2xl font-black text-[10px] uppercase tracking-[0.2em] transition-all flex items-center justify-center shadow-2xl ${
                       deployingId ? 'bg-green-500 text-white shadow-green-500/30' : 'bg-nexus-accent text-white hover:bg-blue-600 shadow-nexus-accent/30'
                     }`}
                   >
                     {deployingId ? (
                       <>
                         <RefreshCw size={16} className="mr-3 animate-spin" /> Orchestrating Cluster...
                       </>
                     ) : (
                       <>
                         <Send size={16} className="mr-3" /> One-Click Autonomous Deployment
                       </>
                     )}
                   </button>
                </div>

                <div className="flex-1 bg-nexus-900 rounded-[2rem] p-6 font-mono text-[11px] border border-nexus-800 shadow-inner overflow-hidden relative group">
                    <div className="absolute top-4 right-6 text-[9px] font-black text-nexus-700 uppercase tracking-widest">Inference_Engine_Log</div>
                    <div className="text-nexus-500 mb-4 flex items-center">
                       <div className="w-1.5 h-1.5 rounded-full bg-green-500 mr-2"></div>
                       ROOT@AGENTPROJECT_OS ~ % /usr/bin/deploy_svc
                    </div>
                    <div className="text-nexus-300 space-y-1.5 h-32 overflow-y-auto custom-scrollbar">
                       <div>[INFO] Initializing Weight_Loader... <span className="text-green-500">DONE</span></div>
                       <div>[INFO] Checkpoint verified: SHA256 (3a1f9c...)</div>
                       <div>[INFO] Allocating VRAM (64201MB) on GPU:0... <span className="text-green-500">OK</span></div>
                       <div className="animate-pulse">[WARM] Scaling pod-replicas (3/3)...</div>
                    </div>
                </div>
             </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default LocalModel;
