import React, { useState, useEffect } from 'react';
import { 
  User, Lock, Settings as SettingsIcon, Bell, Save, Key, Shield, Smartphone, 
  Trash2, Plus, Monitor, Clock, Globe, Moon, Sun, Laptop 
} from 'lucide-react';
import { TRANSLATIONS } from '../constants';
import { Language } from '../types';

interface SettingsProps {
  lang: Language;
  activeTab: string;
}

const Settings: React.FC<SettingsProps> = ({ lang, activeTab: initialTab }) => {
  const t = TRANSLATIONS[lang];
  const [activeTab, setActiveTab] = useState(initialTab || 'profile');
  const [isSaving, setIsSaving] = useState(false);
  const [apiKeys, setApiKeys] = useState([
    { id: 'pk_live_51M...', name: 'Production Cluster', created: '2023-08-15', lastUsed: '2h ago' },
    { id: 'pk_test_84K...', name: 'Dev Environment', created: '2023-09-02', lastUsed: '5m ago' },
  ]);

  useEffect(() => {
    setActiveTab(initialTab);
  }, [initialTab]);

  const handleSave = () => {
    setIsSaving(true);
    setTimeout(() => setIsSaving(false), 1000);
  };

  const handleDeleteKey = (id: string) => {
    setApiKeys(apiKeys.filter(k => k.id !== id));
  };

  const navItems = [
    { id: 'profile', label: t.profile, icon: User },
    { id: 'account', label: t.accountSettings, icon: Lock },
    { id: 'system', label: t.settings, icon: SettingsIcon },
    { id: 'notifications', label: t.notifications, icon: Bell },
  ];

  return (
    <div className="h-full flex flex-col md:flex-row bg-light-surface dark:bg-nexus-800 border border-light-border dark:border-nexus-700 rounded-lg shadow-sm overflow-hidden mb-20">
      
      {/* Sidebar Navigation */}
      <div className="w-full md:w-64 bg-nexus-50 dark:bg-nexus-900 border-r border-light-border dark:border-nexus-700 flex flex-col">
        <div className="p-6 border-b border-light-border dark:border-nexus-700">
           <h2 className="text-xl font-bold text-light-text dark:text-white flex items-center">
              <SettingsIcon className="mr-2 text-nexus-accent" /> {t.settings}
           </h2>
        </div>
        <nav className="flex-1 p-4 space-y-1">
          {navItems.map(item => (
            <button
              key={item.id}
              onClick={() => setActiveTab(item.id)}
              className={`w-full flex items-center px-4 py-3 text-sm font-medium rounded-lg transition-colors ${
                activeTab === item.id 
                  ? 'bg-nexus-200 dark:bg-nexus-800 text-nexus-accent dark:text-white' 
                  : 'text-light-textSec dark:text-nexus-400 hover:bg-nexus-100 dark:hover:bg-nexus-800 hover:text-light-text dark:hover:text-white'
              }`}
            >
              <item.icon size={18} className="mr-3" />
              {item.label}
            </button>
          ))}
        </nav>
      </div>

      {/* Main Content Area */}
      <div className="flex-1 flex flex-col overflow-hidden">
        
        {/* Profile Settings */}
        {activeTab === 'profile' && (
          <div className="flex-1 overflow-y-auto p-8 animate-fade-in">
             <div className="max-w-3xl">
                <div className="mb-8 flex items-center">
                   <div className="w-24 h-24 rounded-full bg-gradient-to-tr from-blue-500 to-purple-500 border-4 border-white dark:border-nexus-700 shadow-lg mr-6"></div>
                   <div>
                      <h3 className="text-2xl font-bold text-light-text dark:text-white">Admin User</h3>
                      <p className="text-light-textSec dark:text-nexus-400">admin@manusproject.io</p>
                      <button className="mt-2 text-sm text-nexus-accent hover:text-blue-400 font-medium">Change Avatar</button>
                   </div>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
                   <div>
                      <label className="block text-sm font-medium text-light-text dark:text-nexus-300 mb-1">{t.fullName}</label>
                      <input 
                         type="text" 
                         defaultValue="Admin User"
                         className="w-full bg-white dark:bg-nexus-900 border border-light-border dark:border-nexus-600 rounded-lg p-2.5 text-sm text-light-text dark:text-white focus:border-nexus-accent outline-none"
                      />
                   </div>
                   <div>
                      <label className="block text-sm font-medium text-light-text dark:text-nexus-300 mb-1">{t.role}</label>
                      <input 
                         type="text" 
                         defaultValue="System Administrator"
                         disabled
                         className="w-full bg-nexus-100 dark:bg-nexus-800/50 border border-light-border dark:border-nexus-700 rounded-lg p-2.5 text-sm text-nexus-500 dark:text-nexus-400 cursor-not-allowed"
                      />
                   </div>
                   <div className="md:col-span-2">
                      <label className="block text-sm font-medium text-light-text dark:text-nexus-300 mb-1">{t.bio}</label>
                      <textarea 
                         rows={4}
                         defaultValue="Lead DevOps Engineer managing the ManusProject infrastructure."
                         className="w-full bg-white dark:bg-nexus-900 border border-light-border dark:border-nexus-600 rounded-lg p-2.5 text-sm text-light-text dark:text-white focus:border-nexus-accent outline-none"
                      />
                   </div>
                   <div>
                      <label className="block text-sm font-medium text-light-text dark:text-nexus-300 mb-1">{t.email}</label>
                      <input 
                         type="email" 
                         defaultValue="admin@manusproject.io"
                         className="w-full bg-white dark:bg-nexus-900 border border-light-border dark:border-nexus-600 rounded-lg p-2.5 text-sm text-light-text dark:text-white focus:border-nexus-accent outline-none"
                      />
                   </div>
                </div>

                <div className="pt-4 border-t border-light-border dark:border-nexus-700 flex justify-end">
                   <button 
                     onClick={handleSave}
                     className="flex items-center px-6 py-2 bg-nexus-accent text-white rounded-lg hover:bg-blue-600 font-medium transition-colors shadow-lg shadow-blue-500/20"
                   >
                      {isSaving ? t.saved : <><Save size={18} className="mr-2" /> {t.saveChanges}</>}
                   </button>
                </div>
             </div>
          </div>
        )}

        {/* Account Settings */}
        {activeTab === 'account' && (
           <div className="flex-1 overflow-y-auto p-8 animate-fade-in">
              <div className="max-w-3xl space-y-10">
                 
                 {/* Password Section */}
                 <div>
                    <h3 className="text-lg font-bold text-light-text dark:text-white mb-4 flex items-center">
                       <Lock size={18} className="mr-2 text-nexus-500" /> {t.changePassword}
                    </h3>
                    <div className="grid grid-cols-1 gap-4 bg-nexus-50 dark:bg-nexus-900/30 p-6 rounded-lg border border-light-border dark:border-nexus-700">
                       <div>
                          <label className="block text-sm font-medium text-light-text dark:text-nexus-300 mb-1">{t.currentPassword}</label>
                          <input type="password" placeholder="••••••••" className="w-full bg-white dark:bg-nexus-900 border border-light-border dark:border-nexus-600 rounded-lg p-2.5 text-sm outline-none focus:border-nexus-accent" />
                       </div>
                       <div className="grid grid-cols-2 gap-4">
                          <div>
                             <label className="block text-sm font-medium text-light-text dark:text-nexus-300 mb-1">{t.newPassword}</label>
                             <input type="password" placeholder="••••••••" className="w-full bg-white dark:bg-nexus-900 border border-light-border dark:border-nexus-600 rounded-lg p-2.5 text-sm outline-none focus:border-nexus-accent" />
                          </div>
                          <div>
                             <label className="block text-sm font-medium text-light-text dark:text-nexus-300 mb-1">{t.confirmPassword}</label>
                             <input type="password" placeholder="••••••••" className="w-full bg-white dark:bg-nexus-900 border border-light-border dark:border-nexus-600 rounded-lg p-2.5 text-sm outline-none focus:border-nexus-accent" />
                          </div>
                       </div>
                       <div className="flex justify-end mt-2">
                          <button className="px-4 py-2 bg-nexus-200 dark:bg-nexus-700 text-light-text dark:text-white rounded hover:bg-nexus-300 dark:hover:bg-nexus-600 text-sm font-medium">Update Password</button>
                       </div>
                    </div>
                 </div>

                 {/* API Keys Section */}
                 <div>
                    <div className="flex justify-between items-center mb-4">
                       <h3 className="text-lg font-bold text-light-text dark:text-white flex items-center">
                          <Key size={18} className="mr-2 text-nexus-500" /> {t.apiKeys}
                       </h3>
                       <button className="flex items-center text-xs px-3 py-1.5 bg-nexus-accent text-white rounded hover:bg-blue-600">
                          <Plus size={14} className="mr-1" /> {t.createKey}
                       </button>
                    </div>
                    <div className="bg-white dark:bg-nexus-900 border border-light-border dark:border-nexus-700 rounded-lg overflow-hidden">
                       <table className="w-full text-left text-sm">
                          <thead className="bg-nexus-50 dark:bg-nexus-800 text-light-textSec dark:text-nexus-400 font-medium">
                             <tr>
                                <th className="p-3">Name</th>
                                <th className="p-3">Key Preview</th>
                                <th className="p-3">Last Used</th>
                                <th className="p-3 text-right">Action</th>
                             </tr>
                          </thead>
                          <tbody className="divide-y divide-light-border dark:divide-nexus-700">
                             {apiKeys.map(key => (
                                <tr key={key.id} className="hover:bg-nexus-50 dark:hover:bg-nexus-800/50">
                                   <td className="p-3 font-medium text-light-text dark:text-white">{key.name}</td>
                                   <td className="p-3 font-mono text-nexus-500">{key.id}</td>
                                   <td className="p-3 text-light-textSec dark:text-nexus-400">{key.lastUsed}</td>
                                   <td className="p-3 text-right">
                                      <button onClick={() => handleDeleteKey(key.id)} className="text-red-500 hover:text-red-600 p-1 hover:bg-red-50 dark:hover:bg-red-900/20 rounded">
                                         <Trash2 size={16} />
                                      </button>
                                   </td>
                                </tr>
                             ))}
                          </tbody>
                       </table>
                    </div>
                 </div>

                 {/* Security Section */}
                 <div>
                    <h3 className="text-lg font-bold text-light-text dark:text-white mb-4 flex items-center">
                       <Shield size={18} className="mr-2 text-nexus-500" /> {t.security}
                    </h3>
                    <div className="bg-nexus-50 dark:bg-nexus-900/30 p-6 rounded-lg border border-light-border dark:border-nexus-700 space-y-6">
                       <div className="flex items-center justify-between">
                          <div className="flex items-center">
                             <div className="p-2 bg-green-500/10 rounded-lg mr-4">
                                <Smartphone size={24} className="text-green-500" />
                             </div>
                             <div>
                                <div className="font-bold text-light-text dark:text-white">{t.twoFactor}</div>
                                <div className="text-sm text-light-textSec dark:text-nexus-400">Secure your account with 2FA</div>
                             </div>
                          </div>
                          <label className="relative inline-flex items-center cursor-pointer">
                             <input type="checkbox" className="sr-only peer" defaultChecked />
                             <div className="w-11 h-6 bg-gray-200 peer-focus:outline-none rounded-full peer dark:bg-nexus-700 peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all dark:border-gray-600 peer-checked:bg-green-500"></div>
                          </label>
                       </div>
                       
                       <div className="pt-4 border-t border-light-border dark:border-nexus-700">
                           <h4 className="text-sm font-bold text-light-text dark:text-nexus-300 mb-3">{t.activeSessions}</h4>
                           <div className="flex items-center justify-between p-3 bg-white dark:bg-nexus-800 rounded border border-light-border dark:border-nexus-600">
                              <div className="flex items-center">
                                 <Monitor size={18} className="text-nexus-400 mr-3" />
                                 <div>
                                    <div className="text-sm font-medium text-light-text dark:text-white">MacBook Pro (This Device)</div>
                                    <div className="text-xs text-light-textSec dark:text-nexus-500">San Francisco, US • Chrome</div>
                                 </div>
                              </div>
                              <div className="text-xs text-green-500 font-bold px-2 py-1 bg-green-500/10 rounded">Active Now</div>
                           </div>
                       </div>
                    </div>
                 </div>

              </div>
           </div>
        )}

        {/* System Settings */}
        {activeTab === 'system' && (
           <div className="flex-1 overflow-y-auto p-8 animate-fade-in">
              <div className="max-w-3xl space-y-8">
                 
                 {/* Appearance */}
                 <div className="bg-light-surface dark:bg-nexus-800 p-6 rounded-lg border border-light-border dark:border-nexus-700 shadow-sm">
                    <h3 className="font-bold text-light-text dark:text-white mb-6 flex items-center">
                       <Monitor size={18} className="mr-2 text-nexus-500" /> {t.appearance}
                    </h3>
                    
                    <div className="space-y-6">
                       <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                          <div>
                             <label className="block text-sm font-medium text-light-text dark:text-nexus-300 mb-2">{t.theme}</label>
                             <div className="grid grid-cols-3 gap-2">
                                <button className="flex flex-col items-center justify-center p-3 border-2 border-transparent hover:border-nexus-accent bg-nexus-50 dark:bg-nexus-900 rounded-lg transition-all">
                                   <Sun size={24} className="mb-2 text-orange-400" />
                                   <span className="text-xs">Light</span>
                                </button>
                                <button className="flex flex-col items-center justify-center p-3 border-2 border-nexus-accent bg-nexus-50 dark:bg-nexus-900 rounded-lg transition-all">
                                   <Moon size={24} className="mb-2 text-blue-400" />
                                   <span className="text-xs">Dark</span>
                                </button>
                                <button className="flex flex-col items-center justify-center p-3 border-2 border-transparent hover:border-nexus-accent bg-nexus-50 dark:bg-nexus-900 rounded-lg transition-all">
                                   <Laptop size={24} className="mb-2 text-nexus-400" />
                                   <span className="text-xs">System</span>
                                </button>
                             </div>
                          </div>
                          <div>
                             <label className="block text-sm font-medium text-light-text dark:text-nexus-300 mb-2">{t.language}</label>
                             <div className="relative">
                                <Globe className="absolute left-3 top-1/2 -translate-y-1/2 text-nexus-500" size={18} />
                                <select className="w-full bg-nexus-50 dark:bg-nexus-900 border border-light-border dark:border-nexus-600 rounded-lg py-2.5 pl-10 pr-4 text-sm outline-none focus:border-nexus-accent appearance-none text-light-text dark:text-white">
                                   <option>English (US)</option>
                                   <option>中文 (简体)</option>
                                   <option>日本語</option>
                                </select>
                             </div>
                          </div>
                       </div>
                    </div>
                 </div>

                 {/* System Policies */}
                 <div className="bg-light-surface dark:bg-nexus-800 p-6 rounded-lg border border-light-border dark:border-nexus-700 shadow-sm">
                     <h3 className="font-bold text-light-text dark:text-white mb-6 flex items-center">
                       <SettingsIcon size={18} className="mr-2 text-nexus-500" /> System Policies
                    </h3>
                    
                    <div className="space-y-6">
                       <div>
                          <div className="flex justify-between mb-2">
                             <label className="text-sm font-medium text-light-text dark:text-nexus-300">{t.logRetention}</label>
                             <span className="text-sm font-mono text-nexus-500">30 Days</span>
                          </div>
                          <input type="range" min="1" max="90" defaultValue="30" className="w-full h-2 bg-nexus-200 dark:bg-nexus-700 rounded-lg appearance-none cursor-pointer" />
                          <div className="flex justify-between text-xs text-nexus-400 mt-1">
                             <span>1 Day</span>
                             <span>90 Days</span>
                          </div>
                       </div>

                       <div className="flex items-center justify-between py-3 border-t border-light-border dark:border-nexus-700">
                          <div>
                             <div className="text-sm font-medium text-light-text dark:text-white">Maintenance Mode</div>
                             <div className="text-xs text-light-textSec dark:text-nexus-500">Suspend all non-essential pipelines</div>
                          </div>
                          <label className="relative inline-flex items-center cursor-pointer">
                             <input type="checkbox" className="sr-only peer" />
                             <div className="w-11 h-6 bg-gray-200 peer-focus:outline-none rounded-full peer dark:bg-nexus-700 peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all dark:border-gray-600 peer-checked:bg-nexus-accent"></div>
                          </label>
                       </div>
                       
                       <div className="flex items-center justify-between py-3 border-t border-light-border dark:border-nexus-700">
                          <div>
                             <div className="text-sm font-medium text-light-text dark:text-white">{t.enableNotifications}</div>
                             <div className="text-xs text-light-textSec dark:text-nexus-500">Receive alerts via Email/Slack</div>
                          </div>
                          <label className="relative inline-flex items-center cursor-pointer">
                             <input type="checkbox" className="sr-only peer" defaultChecked />
                             <div className="w-11 h-6 bg-gray-200 peer-focus:outline-none rounded-full peer dark:bg-nexus-700 peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all dark:border-gray-600 peer-checked:bg-nexus-accent"></div>
                          </label>
                       </div>
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