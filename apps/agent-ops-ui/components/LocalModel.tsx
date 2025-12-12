import React, { useState } from 'react';
import { 
  Upload, Database, Zap, Send, Check, Activity, Search, RefreshCw, Layers 
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
      alert("Deployment Successful! Service is live at localhost:8080");
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
    { subject: 'Inference', A: 70, fullMark: 100 },
    { subject: 'Size', A: 95, fullMark: 100 },
  ];

  return (
    <div className="h-full flex flex-col pb-20">
      
      {/* Tabs Header */}
      <div className="flex border-b border-light-border dark:border-nexus-700 mb-6 bg-light-surface dark:bg-nexus-800 rounded-t-lg">
        <button 
          onClick={() => setActiveTab('fine-tune')}
          className={`px-6 py-3 font-medium text-sm border-b-2 transition-colors flex items-center ${activeTab === 'fine-tune' ? 'border-nexus-accent text-nexus-accent' : 'border-transparent text-light-textSec dark:text-nexus-400 hover:text-light-text dark:hover:text-white'}`}
        >
          <Zap size={16} className="mr-2" /> {t.fineTuning}
        </button>
        <button 
          onClick={() => setActiveTab('data')}
          className={`px-6 py-3 font-medium text-sm border-b-2 transition-colors flex items-center ${activeTab === 'data' ? 'border-nexus-accent text-nexus-accent' : 'border-transparent text-light-textSec dark:text-nexus-400 hover:text-light-text dark:hover:text-white'}`}
        >
          <Database size={16} className="mr-2" /> {t.dataCollection}
        </button>
        <button 
          onClick={() => setActiveTab('deploy')}
          className={`px-6 py-3 font-medium text-sm border-b-2 transition-colors flex items-center ${activeTab === 'deploy' ? 'border-nexus-accent text-nexus-accent' : 'border-transparent text-light-textSec dark:text-nexus-400 hover:text-light-text dark:hover:text-white'}`}
        >
          <Send size={16} className="mr-2" /> {t.deployment}
        </button>
      </div>

      {/* Content Area */}
      <div className="flex-1 overflow-y-auto">
        
        {/* Fine-Tuning Tab */}
        {activeTab === 'fine-tune' && (
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 animate-fade-in">
             <div className="lg:col-span-2 bg-light-surface dark:bg-nexus-800 border border-light-border dark:border-nexus-700 rounded-lg shadow-sm">
                <div className="p-4 border-b border-light-border dark:border-nexus-700 flex justify-between items-center">
                  <h3 className="font-bold text-light-text dark:text-white">Active Tasks</h3>
                  <div className="flex space-x-2">
                     <button className="p-1.5 hover:bg-nexus-100 dark:hover:bg-nexus-700 rounded text-light-textSec dark:text-nexus-400"><RefreshCw size={14}/></button>
                  </div>
                </div>
                <table className="w-full text-left text-sm">
                   <thead className="bg-nexus-50 dark:bg-nexus-900/50 text-light-textSec dark:text-nexus-400">
                      <tr>
                        <th className="p-4 font-medium">Model Name</th>
                        <th className="p-4 font-medium">Base Model</th>
                        <th className="p-4 font-medium">Progress</th>
                        <th className="p-4 font-medium">Epochs</th>
                        <th className="p-4 font-medium">Status</th>
                      </tr>
                   </thead>
                   <tbody className="divide-y divide-light-border dark:divide-nexus-700">
                      {MOCK_LOCAL_MODELS.map(model => (
                        <tr key={model.id} className="hover:bg-nexus-50 dark:hover:bg-nexus-700/30">
                           <td className="p-4 font-medium text-light-text dark:text-white">{model.name}</td>
                           <td className="p-4 text-light-textSec dark:text-nexus-400 font-mono text-xs">{model.baseModel}</td>
                           <td className="p-4">
                              <div className="w-24 bg-nexus-200 dark:bg-nexus-700 h-1.5 rounded-full overflow-hidden">
                                 <div className="bg-nexus-accent h-full" style={{ width: `${(model.epoch / model.totalEpochs) * 100}%` }}></div>
                              </div>
                           </td>
                           <td className="p-4 text-light-textSec dark:text-nexus-400">{model.epoch} / {model.totalEpochs}</td>
                           <td className="p-4">
                              <span className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-bold ${
                                model.status === 'Fine-Tuning' ? 'bg-blue-500/10 text-blue-500' :
                                model.status === 'Deployed' ? 'bg-green-500/10 text-green-500' :
                                'bg-gray-500/10 text-gray-500'
                              }`}>
                                {model.status === 'Fine-Tuning' && <RefreshCw size={10} className="mr-1 animate-spin" />}
                                {model.status}
                              </span>
                           </td>
                        </tr>
                      ))}
                   </tbody>
                </table>
             </div>

             <div className="bg-light-surface dark:bg-nexus-800 border border-light-border dark:border-nexus-700 rounded-lg p-4 shadow-sm flex flex-col">
                <h3 className="font-bold text-light-text dark:text-white mb-4">Training Loss</h3>
                <div className="flex-1 min-h-[250px]">
                   <ResponsiveContainer width="100%" height="100%">
                      <LineChart data={lossData}>
                         <CartesianGrid strokeDasharray="3 3" stroke="#94a3b8" opacity={0.3} vertical={false} />
                         <XAxis dataKey="epoch" stroke="#64748b" tick={{fontSize: 10}} />
                         <YAxis stroke="#64748b" tick={{fontSize: 10}} />
                         <Tooltip 
                           contentStyle={{ borderRadius: '8px', border: 'none', boxShadow: '0 4px 6px -1px rgb(0 0 0 / 0.1)' }}
                         />
                         <Line type="monotone" dataKey="train" stroke="#3b82f6" strokeWidth={2} dot={false} />
                         <Line type="monotone" dataKey="val" stroke="#ef4444" strokeWidth={2} dot={false} />
                      </LineChart>
                   </ResponsiveContainer>
                </div>
                <div className="mt-2 flex justify-center space-x-4 text-xs text-light-textSec dark:text-nexus-400">
                   <div className="flex items-center"><span className="w-2 h-2 rounded-full bg-nexus-accent mr-1"></span> Train</div>
                   <div className="flex items-center"><span className="w-2 h-2 rounded-full bg-nexus-danger mr-1"></span> Val</div>
                </div>
             </div>
          </div>
        )}

        {/* Data Collection Tab */}
        {activeTab === 'data' && (
          <div className="animate-fade-in bg-light-surface dark:bg-nexus-800 border border-light-border dark:border-nexus-700 rounded-lg p-10 text-center">
             <div className="border-2 border-dashed border-light-border dark:border-nexus-600 rounded-lg p-12 hover:border-nexus-accent transition-colors cursor-pointer group">
                <div className="w-16 h-16 bg-nexus-100 dark:bg-nexus-700 rounded-full flex items-center justify-center mx-auto mb-4 group-hover:bg-blue-500/20 group-hover:text-blue-500 transition-colors">
                   <Upload size={32} className="text-nexus-500 dark:text-nexus-300 group-hover:text-blue-500" />
                </div>
                <h3 className="text-lg font-bold text-light-text dark:text-white">Upload Dataset</h3>
                <p className="text-light-textSec dark:text-nexus-400 mt-2 text-sm">Drag & drop CSV, JSONL, or Image folders here</p>
                <button className="mt-6 px-4 py-2 bg-nexus-accent text-white rounded hover:bg-blue-600 transition-colors shadow-md">Browse Files</button>
             </div>
             
             <div className="mt-8 text-left">
                <h4 className="font-bold text-light-text dark:text-white mb-4">Recent Datasets</h4>
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                   {[1, 2, 3, 4].map(i => (
                     <div key={i} className="group relative aspect-square bg-nexus-100 dark:bg-nexus-900 rounded-lg overflow-hidden border border-light-border dark:border-nexus-700">
                        <div className="absolute inset-0 flex items-center justify-center text-nexus-400">
                           <Database size={24} />
                        </div>
                        <div className="absolute bottom-0 left-0 right-0 p-2 bg-black/60 text-white text-xs backdrop-blur-sm">
                           dataset_v{i}.jsonl
                        </div>
                     </div>
                   ))}
                </div>
             </div>
          </div>
        )}

        {/* Deployment Tab */}
        {activeTab === 'deploy' && (
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 animate-fade-in">
             <div className="bg-light-surface dark:bg-nexus-800 border border-light-border dark:border-nexus-700 rounded-lg p-6 shadow-sm">
                <h3 className="font-bold text-light-text dark:text-white mb-6">Model Performance Analysis</h3>
                <div className="h-[300px]">
                   <ResponsiveContainer width="100%" height="100%">
                      <RadarChart outerRadius={90} data={radarData}>
                         <PolarGrid stroke="#94a3b8" opacity={0.3} />
                         <PolarAngleAxis dataKey="subject" tick={{ fill: '#94a3b8', fontSize: 12 }} />
                         <PolarRadiusAxis angle={30} domain={[0, 100]} stroke="transparent" />
                         <Radar name="LLaMA-7B" dataKey="A" stroke="#3b82f6" fill="#3b82f6" fillOpacity={0.3} />
                         <Tooltip />
                      </RadarChart>
                   </ResponsiveContainer>
                </div>
             </div>

             <div className="bg-light-surface dark:bg-nexus-800 border border-light-border dark:border-nexus-700 rounded-lg p-6 shadow-sm flex flex-col justify-between">
                <div>
                   <h3 className="font-bold text-light-text dark:text-white mb-2">Deploy to Production</h3>
                   <p className="text-sm text-light-textSec dark:text-nexus-400 mb-6">Publish the selected model to the local Kubernetes cluster inference endpoint.</p>
                   
                   <div className="space-y-4 mb-8">
                      <div className="bg-nexus-50 dark:bg-nexus-900/50 p-4 rounded border border-light-border dark:border-nexus-700">
                         <label className="text-xs text-nexus-500 uppercase font-bold">Target Endpoint</label>
                         <div className="text-light-text dark:text-white font-mono mt-1">http://localhost:8080/v1/models/llama-finetune</div>
                      </div>
                      <div className="bg-nexus-50 dark:bg-nexus-900/50 p-4 rounded border border-light-border dark:border-nexus-700">
                         <label className="text-xs text-nexus-500 uppercase font-bold">Resources</label>
                         <div className="text-light-text dark:text-white font-mono mt-1">2x NVIDIA A100, 16GB RAM</div>
                      </div>
                   </div>
                </div>
                
                <button 
                  onClick={() => handleDeploy('m1')}
                  disabled={!!deployingId}
                  className={`w-full py-3 rounded font-bold text-white transition-all flex items-center justify-center ${
                    deployingId ? 'bg-nexus-success cursor-wait' : 'bg-nexus-accent hover:bg-blue-600 shadow-lg shadow-blue-500/20'
                  }`}
                >
                  {deployingId ? (
                    <>
                      <RefreshCw size={18} className="mr-2 animate-spin" /> Deploying...
                    </>
                  ) : (
                    <>
                      <Send size={18} className="mr-2" /> One-Click Publish
                    </>
                  )}
                </button>
             </div>
             
             {/* CLI Simulation */}
             <div className="lg:col-span-2 bg-nexus-900 rounded-lg p-4 font-mono text-sm border border-nexus-700 shadow-inner max-h-48 overflow-y-auto">
                 <div className="text-nexus-500 mb-2"># Local Deployment Simulation Console</div>
                 <div className="text-green-400">➜  ~ huggingface-cli fine-tune --model llama-2-7b --dataset finance-corpus-v1</div>
                 <div className="text-nexus-300 mt-1">Initializing training environment... Done.</div>
                 <div className="text-nexus-300">Loading dataset shards (1/4)...</div>
                 <div className="text-nexus-300">Allocating tensors on GPU:0...</div>
             </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default LocalModel;