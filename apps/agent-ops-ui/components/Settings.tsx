
import React, { useState, useEffect } from 'react';
import { 
  User, Lock, Settings as SettingsIcon, Bell, Save, Key, Shield, Smartphone, 
  Trash2, Plus, Monitor, Clock, Globe, Moon, Sun, Laptop, Mail, Slack, Zap, CheckCircle, AlertTriangle
} from 'lucide-react';
import { TRANSLATIONS } from '../constants';
import { Language } from '../types';

interface SettingsProps {
  lang: Language;
  activeTab: string;
}

const AVATARS = [
  { id: 1, name: 'Rick Sanchez', url: 'https://rickandmortyapi.com/api/character/avatar/1.jpeg' },
  { id: 2, name: 'Morty Smith', url: 'https://rickandmortyapi.com/api/character/avatar/2.jpeg' },
  { id: 3, name: 'Summer Smith', url: 'https://rickandmortyapi.com/api/character/avatar/3.jpeg' },
  { id: 4, name: 'Beth Smith', url: 'https://rickandmortyapi.com/api/character/avatar/4.jpeg' },
  { id: 5, name: 'Jerry Smith', url: 'https://rickandmortyapi.com/api/character/avatar/5.jpeg' },
];

const Settings: React.FC<SettingsProps> = ({ lang, activeTab }) => {
  const t = TRANSLATIONS[lang];
  const [isSaving, setIsSaving] = useState(false);
  const [selectedAvatar, setSelectedAvatar] = useState(AVATARS[0].url);
  
  const currentYear = new Date().getFullYear();
  const [apiKeys, setApiKeys] = useState([
    { id: 'pk_live_51M...', name: 'Production Cluster', created: `${currentYear}-08-15`, lastUsed: '2h ago' },
    { id: 'pk_test_84K...', name: 'Dev Environment', created: `${currentYear}-09-02`, lastUsed: '5m ago' },
  ]);

  const [notifications, setNotifications] = useState({
    email: true,
    slack: false,
    webhook: true,
    alerts: {
      deployment: true,
      error: true,
      security: true
    }
  });

  const handleSave = () => {
    setIsSaving(true);
    setTimeout(() => setIsSaving(false), 1000);
  };

  const handleDeleteKey = (id: string) => {
    setApiKeys(apiKeys.filter(k => k.id !== id));
  };

  const toggleNotification = (key: keyof typeof notifications) => {
    setNotifications(prev => ({ ...prev, [key]: !prev[key] }));
  };

  const getHeaderInfo = () => {
    switch (activeTab) {
      case 'profile':
        return {
          title: t.profile,
          description: "Manage your personal information, public profile, and identity verification.",
          icon: User,
          color: "text-blue-500",
          bg: "bg-blue-500/10"
        };
      case 'account':
        return {
          title: t.accountSettings,
          description: "Configure security settings, authentication methods, and API access keys.",
          icon: Lock,
          color: "text-purple-500",
          bg: "bg-purple-500/10"
        };
      case 'notifications':
        return {
          title: t.notifications,
          description: "Manage alert channels, frequency, and subscription preferences.",
          icon: Bell,
          color: "text-orange-500",
          bg: "bg-orange-500/10"
        };
      case 'system':
      default:
        return {
          title: t.settings,
          description: "Global system preferences, appearance, and operational policies.",
          icon: SettingsIcon,
          color: "text-nexus-accent",
          bg: "bg-nexus-accent/10"
        };
    }
  };

  const header = getHeaderInfo();

  return (
    <div className="h-full flex flex-col pb-20 animate-fade-in relative">
      
      {/* Page Header */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-6 mb-10 bg-white/60 dark:bg-nexus-800/60 backdrop-blur-xl p-8 rounded-[2.5rem] border border-slate-200 dark:border-nexus-700/50 shadow-sm">
        <div className="flex items-center">
          <div className={`p-5 rounded-[1.5rem] mr-6 ${header.bg} ${header.color}`}>
             <header.icon size={32} />
          </div>
          <div>
             <h1 className="text-3xl font-black text-slate-900 dark:text-white tracking-tight">{header.title}</h1>
             <p className="text-slate-500 dark:text-nexus-400 font-medium mt-1 text-sm">{header.description}</p>
          </div>
        </div>
        <button 
           onClick={handleSave}
           className="flex items-center px-8 py-4 bg-nexus-accent text-white rounded-2xl hover:bg-blue-600 font-black text-xs uppercase tracking-[0.2em] transition-all shadow-xl shadow-nexus-accent/20 active:scale-95"
        >
           {isSaving ? (
             <span className="flex items-center"><Clock size={16} className="mr-3 animate-spin" /> {t.saved}</span>
           ) : (
             <span className="flex items-center"><Save size={16} className="mr-3" /> {t.saveChanges}</span>
           )}
        </button>
      </div>

      {/* Main Content View */}
      <div className="flex-1 overflow-y-auto custom-scrollbar">
        
        {/* === PROFILE VIEW === */}
        {activeTab === 'profile' && (
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-8 animate-slide-up">
             {/* Profile Card */}
             <div className="lg:col-span-1">
                <div className="bg-white dark:bg-nexus-800 border border-slate-200 dark:border-nexus-700 rounded-[2.5rem] overflow-hidden shadow-sm sticky top-0">
                   <div className="h-32 bg-gradient-to-r from-blue-500 to-purple-600"></div>
                   <div className="px-8 pb-8 relative">
                      <div className="w-24 h-24 rounded-[1.5rem] bg-white dark:bg-nexus-800 p-1.5 absolute -top-12 shadow-xl">
                         <img 
                           src={selectedAvatar} 
                           alt="Profile" 
                           className="w-full h-full rounded-[1.2rem] object-cover bg-slate-200 dark:bg-nexus-700"
                         />
                      </div>
                      <div className="mt-14">
                         <h3 className="text-2xl font-black text-slate-900 dark:text-white">Admin User</h3>
                         <p className="text-slate-500 dark:text-nexus-400 font-medium">SysAdmin • DevOps Lead</p>
                         
                         <div className="mt-6 flex flex-wrap gap-2">
                            <span className="px-3 py-1 bg-green-500/10 text-green-500 rounded-lg text-[10px] font-black uppercase tracking-wider">Verified</span>
                            <span className="px-3 py-1 bg-blue-500/10 text-blue-500 rounded-lg text-[10px] font-black uppercase tracking-wider">MFA Active</span>
                         </div>

                         <div className="mt-8 pt-8 border-t border-slate-100 dark:border-nexus-700 space-y-4">
                            <div className="flex items-center text-sm text-slate-600 dark:text-nexus-300">
                               <Mail size={16} className="mr-3 text-nexus-400" /> admin@agentproject.io
                            </div>
                            <div className="flex items-center text-sm text-slate-600 dark:text-nexus-300">
                               <Globe size={16} className="mr-3 text-nexus-400" /> San Francisco, CA
                            </div>
                         </div>
                      </div>
                   </div>
                </div>
             </div>

             {/* Edit Form */}
             <div className="lg:col-span-2 space-y-8">
                <div className="bg-white dark:bg-nexus-800 border border-slate-200 dark:border-nexus-700 rounded-[2.5rem] p-10 shadow-sm">
                   <h3 className="text-lg font-black text-slate-900 dark:text-white mb-8 flex items-center">
                      <User size={20} className="mr-3 text-nexus-accent" />
                      Personal Information
                   </h3>
                   
                   {/* Avatar Selection */}
                   <div className="mb-8">
                      <label className="block text-[10px] font-black uppercase tracking-widest text-slate-400 dark:text-nexus-500 mb-4">Select Avatar</label>
                      <div className="flex flex-wrap gap-4">
                        {AVATARS.map((avatar) => (
                           <button 
                              key={avatar.id} 
                              onClick={() => setSelectedAvatar(avatar.url)}
                              className={`relative w-16 h-16 rounded-2xl overflow-hidden transition-all duration-300 border-2 ${
                                 selectedAvatar === avatar.url 
                                 ? 'border-nexus-accent scale-110 shadow-lg shadow-nexus-accent/20' 
                                 : 'border-transparent opacity-70 hover:opacity-100 hover:scale-105'
                              }`}
                           >
                              <img src={avatar.url} alt={avatar.name} className="w-full h-full object-cover" />
                              {selectedAvatar === avatar.url && (
                                <div className="absolute inset-0 bg-nexus-accent/20 flex items-center justify-center">
                                   <CheckCircle size={20} className="text-white drop-shadow-md" />
                                </div>
                              )}
                           </button>
                        ))}
                      </div>
                   </div>

                   <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
                      <div>
                         <label className="block text-[10px] font-black uppercase tracking-widest text-slate-400 dark:text-nexus-500 mb-2">{t.fullName}</label>
                         <input 
                            type="text" 
                            defaultValue="Admin User"
                            className="w-full bg-slate-50 dark:bg-nexus-900/50 border border-slate-200 dark:border-nexus-600 rounded-xl p-4 text-sm text-slate-900 dark:text-white focus:border-nexus-accent focus:ring-4 focus:ring-nexus-accent/10 outline-none transition-all font-bold"
                         />
                      </div>
                      <div>
                         <label className="block text-[10px] font-black uppercase tracking-widest text-slate-400 dark:text-nexus-500 mb-2">{t.role}</label>
                         <input 
                            type="text" 
                            defaultValue="System Administrator"
                            disabled
                            className="w-full bg-slate-100 dark:bg-nexus-900 border border-slate-200 dark:border-nexus-700 rounded-xl p-4 text-sm text-slate-500 dark:text-nexus-400 cursor-not-allowed font-bold"
                         />
                      </div>
                      <div className="md:col-span-2">
                         <label className="block text-[10px] font-black uppercase tracking-widest text-slate-400 dark:text-nexus-500 mb-2">{t.bio}</label>
                         <textarea 
                            rows={4}
                            defaultValue="Lead DevOps Engineer managing the AgentProject infrastructure. Responsible for cluster stability and AI pipeline orchestration."
                            className="w-full bg-slate-50 dark:bg-nexus-900/50 border border-slate-200 dark:border-nexus-600 rounded-xl p-4 text-sm text-slate-900 dark:text-white focus:border-nexus-accent focus:ring-4 focus:ring-nexus-accent/10 outline-none transition-all font-medium leading-relaxed"
                         />
                      </div>
                      <div>
                         <label className="block text-[10px] font-black uppercase tracking-widest text-slate-400 dark:text-nexus-500 mb-2">{t.email}</label>
                         <input 
                            type="email" 
                            defaultValue="admin@agentproject.io"
                            className="w-full bg-slate-50 dark:bg-nexus-900/50 border border-slate-200 dark:border-nexus-600 rounded-xl p-4 text-sm text-slate-900 dark:text-white focus:border-nexus-accent focus:ring-4 focus:ring-nexus-accent/10 outline-none transition-all font-bold"
                         />
                      </div>
                   </div>
                </div>
             </div>
          </div>
        )}

        {/* === ACCOUNT VIEW === */}
        {activeTab === 'account' && (
           <div className="space-y-8 animate-slide-up">
              <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
                 {/* Password Section */}
                 <div className="bg-white dark:bg-nexus-800 border border-slate-200 dark:border-nexus-700 rounded-[2.5rem] p-10 shadow-sm">
                    <h3 className="text-lg font-black text-slate-900 dark:text-white mb-8 flex items-center">
                       <Lock size={20} className="mr-3 text-purple-500" /> {t.changePassword}
                    </h3>
                    <div className="space-y-6">
                       <div>
                          <label className="block text-[10px] font-black uppercase tracking-widest text-slate-400 dark:text-nexus-500 mb-2">{t.currentPassword}</label>
                          <input type="password" placeholder="••••••••" className="w-full bg-slate-50 dark:bg-nexus-900/50 border border-slate-200 dark:border-nexus-600 rounded-xl p-4 text-sm focus:border-nexus-accent focus:ring-4 focus:ring-nexus-accent/10 outline-none transition-all" />
                       </div>
                       <div className="grid grid-cols-2 gap-6">
                          <div>
                             <label className="block text-[10px] font-black uppercase tracking-widest text-slate-400 dark:text-nexus-500 mb-2">{t.newPassword}</label>
                             <input type="password" placeholder="••••••••" className="w-full bg-slate-50 dark:bg-nexus-900/50 border border-slate-200 dark:border-nexus-600 rounded-xl p-4 text-sm focus:border-nexus-accent focus:ring-4 focus:ring-nexus-accent/10 outline-none transition-all" />
                          </div>
                          <div>
                             <label className="block text-[10px] font-black uppercase tracking-widest text-slate-400 dark:text-nexus-500 mb-2">{t.confirmPassword}</label>
                             <input type="password" placeholder="••••••••" className="w-full bg-slate-50 dark:bg-nexus-900/50 border border-slate-200 dark:border-nexus-600 rounded-xl p-4 text-sm focus:border-nexus-accent focus:ring-4 focus:ring-nexus-accent/10 outline-none transition-all" />
                          </div>
                       </div>
                       <div className="pt-4 flex justify-end">
                           <button className="px-6 py-3 bg-slate-100 dark:bg-nexus-700 hover:bg-slate-200 dark:hover:bg-nexus-600 rounded-xl text-xs font-black uppercase tracking-widest text-slate-700 dark:text-white transition-colors">
                              Update Password
                           </button>
                       </div>
                    </div>
                 </div>

                 {/* Security Section */}
                 <div className="bg-white dark:bg-nexus-800 border border-slate-200 dark:border-nexus-700 rounded-[2.5rem] p-10 shadow-sm flex flex-col justify-between">
                    <div>
                       <h3 className="text-lg font-black text-slate-900 dark:text-white mb-8 flex items-center">
                          <Shield size={20} className="mr-3 text-green-500" /> {t.security}
                       </h3>
                       <div className="bg-green-500/5 border border-green-500/20 rounded-2xl p-6 flex items-start mb-6">
                          <div className="p-3 bg-green-500/10 rounded-xl mr-4">
                             <Smartphone size={24} className="text-green-500" />
                          </div>
                          <div className="flex-1">
                             <div className="flex items-center justify-between mb-1">
                                <h4 className="font-bold text-slate-900 dark:text-white">{t.twoFactor}</h4>
                                <div className="relative inline-flex items-center cursor-pointer">
                                   <input type="checkbox" className="sr-only peer" defaultChecked />
                                   <div className="w-11 h-6 bg-slate-200 peer-focus:outline-none rounded-full peer dark:bg-nexus-700 peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all dark:border-gray-600 peer-checked:bg-green-500"></div>
                                </div>
                             </div>
                             <p className="text-sm text-slate-500 dark:text-nexus-400">Protect your account with an extra layer of security using your mobile device.</p>
                          </div>
                       </div>
                    </div>
                    
                    <div className="border-t border-slate-100 dark:border-nexus-700 pt-6">
                       <h4 className="text-[10px] font-black uppercase tracking-widest text-slate-400 dark:text-nexus-500 mb-4">{t.activeSessions}</h4>
                       <div className="flex items-center justify-between p-4 bg-slate-50 dark:bg-nexus-900/50 rounded-2xl border border-slate-100 dark:border-nexus-700">
                          <div className="flex items-center">
                             <Monitor size={20} className="text-nexus-400 mr-4" />
                             <div>
                                <div className="text-sm font-bold text-slate-900 dark:text-white">MacBook Pro (M3 Max)</div>
                                <div className="text-xs text-slate-500 dark:text-nexus-500 font-mono mt-0.5">San Francisco, US • Chrome 124.0</div>
                             </div>
                          </div>
                          <div className="flex items-center space-x-2 text-xs text-green-500 font-black uppercase tracking-wider bg-green-500/10 px-3 py-1.5 rounded-lg">
                             <span className="w-2 h-2 bg-green-500 rounded-full animate-pulse"></span>
                             <span>Active</span>
                          </div>
                       </div>
                    </div>
                 </div>
              </div>

              {/* API Keys Table */}
              <div className="bg-white dark:bg-nexus-800 border border-slate-200 dark:border-nexus-700 rounded-[2.5rem] p-10 shadow-sm">
                 <div className="flex justify-between items-center mb-8">
                    <h3 className="text-lg font-black text-slate-900 dark:text-white flex items-center">
                       <Key size={20} className="mr-3 text-orange-500" /> {t.apiKeys}
                    </h3>
                    <button className="flex items-center text-xs font-black uppercase tracking-widest px-4 py-2 bg-slate-900 dark:bg-white text-white dark:text-nexus-900 rounded-xl hover:opacity-90 transition-opacity">
                       <Plus size={14} className="mr-2" /> {t.createKey}
                    </button>
                 </div>
                 <div className="overflow-hidden rounded-2xl border border-slate-200 dark:border-nexus-700">
                    <table className="w-full text-left text-sm">
                       <thead className="bg-slate-50 dark:bg-nexus-900/50 text-[10px] uppercase font-black tracking-widest text-slate-400 dark:text-nexus-500">
                          <tr>
                             <th className="p-5">Key Name</th>
                             <th className="p-5">Token Prefix</th>
                             <th className="p-5">Created</th>
                             <th className="p-5">Last Used</th>
                             <th className="p-5 text-right">Action</th>
                          </tr>
                       </thead>
                       <tbody className="divide-y divide-slate-100 dark:divide-nexus-700/50 bg-white dark:bg-nexus-800">
                          {apiKeys.map(key => (
                             <tr key={key.id} className="hover:bg-slate-50 dark:hover:bg-nexus-700/20 transition-colors">
                                <td className="p-5 font-bold text-slate-900 dark:text-white">{key.name}</td>
                                <td className="p-5 font-mono text-nexus-500 bg-slate-50 dark:bg-transparent">{key.id}</td>
                                <td className="p-5 text-slate-500 dark:text-nexus-400">{key.created}</td>
                                <td className="p-5 text-slate-500 dark:text-nexus-400 flex items-center">
                                   <div className="w-2 h-2 rounded-full bg-green-500 mr-2"></div>
                                   {key.lastUsed}
                                </td>
                                <td className="p-5 text-right">
                                   <button onClick={() => handleDeleteKey(key.id)} className="text-slate-400 hover:text-red-500 p-2 hover:bg-red-50 dark:hover:bg-red-900/20 rounded-lg transition-all">
                                      <Trash2 size={16} />
                                   </button>
                                </td>
                             </tr>
                          ))}
                       </tbody>
                    </table>
                 </div>
              </div>
           </div>
        )}

        {/* === NOTIFICATIONS VIEW === */}
        {activeTab === 'notifications' && (
           <div className="space-y-8 animate-slide-up">
              <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
                 {/* Channel: Email */}
                 <div className="bg-white dark:bg-nexus-800 border border-slate-200 dark:border-nexus-700 rounded-[2.5rem] p-8 relative overflow-hidden group">
                    <div className="absolute top-0 right-0 p-8 opacity-10 group-hover:opacity-20 transition-opacity">
                       <Mail size={80} />
                    </div>
                    <div className="relative z-10">
                       <div className="w-12 h-12 bg-blue-500/10 rounded-2xl flex items-center justify-center mb-6 text-blue-500">
                          <Mail size={24} />
                       </div>
                       <h3 className="text-lg font-black text-slate-900 dark:text-white mb-2">Email Reports</h3>
                       <p className="text-sm text-slate-500 dark:text-nexus-400 mb-6">Receive daily summaries and critical alerts via email.</p>
                       <div className="flex items-center justify-between">
                          <span className="text-xs font-bold text-slate-400 uppercase tracking-wider">Status</span>
                          <label className="relative inline-flex items-center cursor-pointer">
                             <input type="checkbox" className="sr-only peer" checked={notifications.email} onChange={() => toggleNotification('email')} />
                             <div className="w-11 h-6 bg-slate-200 peer-focus:outline-none rounded-full peer dark:bg-nexus-700 peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all dark:border-gray-600 peer-checked:bg-blue-500"></div>
                          </label>
                       </div>
                    </div>
                 </div>

                 {/* Channel: Slack */}
                 <div className="bg-white dark:bg-nexus-800 border border-slate-200 dark:border-nexus-700 rounded-[2.5rem] p-8 relative overflow-hidden group">
                    <div className="absolute top-0 right-0 p-8 opacity-10 group-hover:opacity-20 transition-opacity">
                       <Slack size={80} />
                    </div>
                    <div className="relative z-10">
                       <div className="w-12 h-12 bg-purple-500/10 rounded-2xl flex items-center justify-center mb-6 text-purple-500">
                          <Slack size={24} />
                       </div>
                       <h3 className="text-lg font-black text-slate-900 dark:text-white mb-2">Slack Integration</h3>
                       <p className="text-sm text-slate-500 dark:text-nexus-400 mb-6">Real-time incident streaming to your #ops channel.</p>
                       <div className="flex items-center justify-between">
                          <span className="text-xs font-bold text-slate-400 uppercase tracking-wider">Status</span>
                          <label className="relative inline-flex items-center cursor-pointer">
                             <input type="checkbox" className="sr-only peer" checked={notifications.slack} onChange={() => toggleNotification('slack')} />
                             <div className="w-11 h-6 bg-slate-200 peer-focus:outline-none rounded-full peer dark:bg-nexus-700 peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all dark:border-gray-600 peer-checked:bg-purple-500"></div>
                          </label>
                       </div>
                    </div>
                 </div>

                 {/* Channel: Webhook */}
                 <div className="bg-white dark:bg-nexus-800 border border-slate-200 dark:border-nexus-700 rounded-[2.5rem] p-8 relative overflow-hidden group">
                    <div className="absolute top-0 right-0 p-8 opacity-10 group-hover:opacity-20 transition-opacity">
                       <Zap size={80} />
                    </div>
                    <div className="relative z-10">
                       <div className="w-12 h-12 bg-yellow-500/10 rounded-2xl flex items-center justify-center mb-6 text-yellow-500">
                          <Zap size={24} />
                       </div>
                       <h3 className="text-lg font-black text-slate-900 dark:text-white mb-2">Custom Webhook</h3>
                       <p className="text-sm text-slate-500 dark:text-nexus-400 mb-6">Push events to external PagerDuty or custom endpoints.</p>
                       <div className="flex items-center justify-between">
                          <span className="text-xs font-bold text-slate-400 uppercase tracking-wider">Status</span>
                          <label className="relative inline-flex items-center cursor-pointer">
                             <input type="checkbox" className="sr-only peer" checked={notifications.webhook} onChange={() => toggleNotification('webhook')} />
                             <div className="w-11 h-6 bg-slate-200 peer-focus:outline-none rounded-full peer dark:bg-nexus-700 peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all dark:border-gray-600 peer-checked:bg-yellow-500"></div>
                          </label>
                       </div>
                    </div>
                 </div>
              </div>

              {/* Detailed Alert Preferences */}
              <div className="bg-white dark:bg-nexus-800 border border-slate-200 dark:border-nexus-700 rounded-[2.5rem] p-10 shadow-sm">
                 <h3 className="text-lg font-black text-slate-900 dark:text-white mb-8 flex items-center">
                    <Bell size={20} className="mr-3 text-orange-500" /> Alert Subscriptions
                 </h3>
                 <div className="space-y-6">
                    <div className="flex items-center justify-between p-4 bg-slate-50 dark:bg-nexus-900/50 rounded-2xl border border-slate-100 dark:border-nexus-700/50">
                       <div className="flex items-center">
                          <div className="p-2 bg-green-500/10 rounded-lg mr-4 text-green-500"><CheckCircle size={20}/></div>
                          <div>
                             <div className="text-sm font-bold text-slate-900 dark:text-white">Successful Deployments</div>
                             <div className="text-xs text-slate-500 dark:text-nexus-500">Notify when a pipeline completes successfully.</div>
                          </div>
                       </div>
                       <label className="relative inline-flex items-center cursor-pointer">
                          <input type="checkbox" className="sr-only peer" defaultChecked />
                          <div className="w-11 h-6 bg-slate-200 peer-focus:outline-none rounded-full peer dark:bg-nexus-700 peer-checked:after:translate-x-full peer-checked:bg-nexus-accent after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:rounded-full after:h-5 after:w-5 after:transition-all"></div>
                       </label>
                    </div>

                    <div className="flex items-center justify-between p-4 bg-slate-50 dark:bg-nexus-900/50 rounded-2xl border border-slate-100 dark:border-nexus-700/50">
                       <div className="flex items-center">
                          <div className="p-2 bg-red-500/10 rounded-lg mr-4 text-red-500"><AlertTriangle size={20}/></div>
                          <div>
                             <div className="text-sm font-bold text-slate-900 dark:text-white">Critical Errors</div>
                             <div className="text-xs text-slate-500 dark:text-nexus-500">Immediate alerts for 5xx errors and pod crashes.</div>
                          </div>
                       </div>
                       <label className="relative inline-flex items-center cursor-pointer">
                          <input type="checkbox" className="sr-only peer" defaultChecked />
                          <div className="w-11 h-6 bg-slate-200 peer-focus:outline-none rounded-full peer dark:bg-nexus-700 peer-checked:after:translate-x-full peer-checked:bg-nexus-accent after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:rounded-full after:h-5 after:w-5 after:transition-all"></div>
                       </label>
                    </div>

                    <div className="flex items-center justify-between p-4 bg-slate-50 dark:bg-nexus-900/50 rounded-2xl border border-slate-100 dark:border-nexus-700/50">
                       <div className="flex items-center">
                          <div className="p-2 bg-purple-500/10 rounded-lg mr-4 text-purple-500"><Shield size={20}/></div>
                          <div>
                             <div className="text-sm font-bold text-slate-900 dark:text-white">Security Audits</div>
                             <div className="text-xs text-slate-500 dark:text-nexus-500">Weekly reports on access logs and failed logins.</div>
                          </div>
                       </div>
                       <label className="relative inline-flex items-center cursor-pointer">
                          <input type="checkbox" className="sr-only peer" defaultChecked />
                          <div className="w-11 h-6 bg-slate-200 peer-focus:outline-none rounded-full peer dark:bg-nexus-700 peer-checked:after:translate-x-full peer-checked:bg-nexus-accent after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:rounded-full after:h-5 after:w-5 after:transition-all"></div>
                       </label>
                    </div>
                 </div>
              </div>
           </div>
        )}

        {/* === SYSTEM VIEW === */}
        {activeTab === 'system' && (
           <div className="space-y-8 animate-slide-up">
              
              {/* Appearance */}
              <div className="bg-white dark:bg-nexus-800 border border-slate-200 dark:border-nexus-700 rounded-[2.5rem] p-10 shadow-sm">
                 <h3 className="text-lg font-black text-slate-900 dark:text-white mb-8 flex items-center">
                    <Monitor size={20} className="mr-3 text-nexus-accent" /> {t.appearance}
                 </h3>
                 <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
                    <div>
                       <label className="block text-[10px] font-black uppercase tracking-widest text-slate-400 dark:text-nexus-500 mb-4">{t.theme}</label>
                       <div className="grid grid-cols-3 gap-4">
                          <button className="flex flex-col items-center justify-center p-4 border-2 border-slate-100 dark:border-nexus-700 hover:border-nexus-accent bg-slate-50 dark:bg-nexus-900/50 rounded-2xl transition-all group">
                             <Sun size={28} className="mb-3 text-orange-400 group-hover:scale-110 transition-transform" />
                             <span className="text-xs font-bold text-slate-600 dark:text-nexus-300">Light</span>
                          </button>
                          <button className="flex flex-col items-center justify-center p-4 border-2 border-nexus-accent bg-blue-50/50 dark:bg-nexus-900/50 rounded-2xl transition-all group shadow-inner">
                             <Moon size={28} className="mb-3 text-blue-500 group-hover:scale-110 transition-transform" />
                             <span className="text-xs font-bold text-slate-900 dark:text-white">Dark</span>
                          </button>
                          <button className="flex flex-col items-center justify-center p-4 border-2 border-slate-100 dark:border-nexus-700 hover:border-nexus-accent bg-slate-50 dark:bg-nexus-900/50 rounded-2xl transition-all group">
                             <Laptop size={28} className="mb-3 text-slate-400 group-hover:scale-110 transition-transform" />
                             <span className="text-xs font-bold text-slate-600 dark:text-nexus-300">System</span>
                          </button>
                       </div>
                    </div>
                    <div>
                       <label className="block text-[10px] font-black uppercase tracking-widest text-slate-400 dark:text-nexus-500 mb-4">{t.language}</label>
                       <div className="relative">
                          <Globe className="absolute left-4 top-1/2 -translate-y-1/2 text-nexus-500" size={18} />
                          <select className="w-full bg-slate-50 dark:bg-nexus-900/50 border border-slate-200 dark:border-nexus-600 rounded-2xl py-4 pl-12 pr-4 text-sm font-bold outline-none focus:border-nexus-accent appearance-none text-slate-900 dark:text-white transition-all">
                             <option>English (United States)</option>
                             <option>中文 (简体)</option>
                             <option>日本語 (Japanese)</option>
                          </select>
                       </div>
                       <p className="text-xs text-slate-400 dark:text-nexus-500 mt-3 pl-1">
                          Platform language updates immediately.
                       </p>
                    </div>
                 </div>
              </div>

              {/* System Policies */}
              <div className="bg-white dark:bg-nexus-800 border border-slate-200 dark:border-nexus-700 rounded-[2.5rem] p-10 shadow-sm">
                  <h3 className="text-lg font-black text-slate-900 dark:text-white mb-8 flex items-center">
                    <SettingsIcon size={20} className="mr-3 text-nexus-accent" /> Operational Policies
                 </h3>
                 
                 <div className="space-y-8">
                    <div>
                       <div className="flex justify-between mb-4">
                          <label className="text-sm font-bold text-slate-900 dark:text-white">{t.logRetention}</label>
                          <span className="text-sm font-mono text-nexus-500 font-bold bg-nexus-100 dark:bg-nexus-900 px-2 py-1 rounded">30 Days</span>
                       </div>
                       <input type="range" min="1" max="90" defaultValue="30" className="w-full h-2 bg-slate-200 dark:bg-nexus-700 rounded-lg appearance-none cursor-pointer accent-nexus-accent" />
                       <div className="flex justify-between text-[10px] font-black uppercase tracking-wider text-slate-400 dark:text-nexus-500 mt-2">
                          <span>1 Day (Dev)</span>
                          <span>90 Days (Compliance)</span>
                       </div>
                    </div>

                    <div className="flex items-center justify-between p-6 bg-slate-50 dark:bg-nexus-900/50 rounded-2xl border border-slate-100 dark:border-nexus-700/50">
                       <div>
                          <div className="text-sm font-bold text-slate-900 dark:text-white">Maintenance Mode</div>
                          <div className="text-xs text-slate-500 dark:text-nexus-500 mt-1">Suspend all non-essential pipelines and prevent new deployments.</div>
                       </div>
                       <label className="relative inline-flex items-center cursor-pointer">
                          <input type="checkbox" className="sr-only peer" />
                          <div className="w-14 h-8 bg-slate-200 peer-focus:outline-none rounded-full peer dark:bg-nexus-700 peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[4px] after:left-[4px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-6 after:w-6 after:transition-all dark:border-gray-600 peer-checked:bg-nexus-accent"></div>
                       </label>
                    </div>
                 </div>
              </div>

           </div>
        )}

      </div>
    </div>
  );
};

export default Settings;
