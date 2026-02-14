
import React, { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { useStore } from '../store';
import { Icons } from './icons';
import clsx from 'clsx';
import { translations } from '../locales';
import { Globe, Code2, Image as ImageIcon, Database, ShieldAlert, Cpu, Volume2, Sparkles, MessageSquare, Upload } from 'lucide-react';

const ModalBackdrop: React.FC<{ children: React.ReactNode; onClose: () => void }> = ({ children, onClose }) => (
  <motion.div 
    initial={{ opacity: 0 }}
    animate={{ opacity: 1 }}
    exit={{ opacity: 0 }}
    className="fixed inset-0 z-[60] bg-black/40 backdrop-blur-sm flex items-center justify-center p-4"
    onClick={onClose}
  >
    {children}
  </motion.div>
);

const ModalContent: React.FC<{ children: React.ReactNode; title: string; icon?: any; onClose?: () => void; className?: string }> = ({ children, title, icon: Icon, onClose, className }) => (
  <motion.div 
    initial={{ scale: 0.95, opacity: 0, y: 20 }}
    animate={{ scale: 1, opacity: 1, y: 0 }}
    exit={{ scale: 0.95, opacity: 0, y: 20 }}
    transition={{ type: "spring", stiffness: 300, damping: 30 }}
    className={clsx(
        "bg-white w-full rounded-[28px] shadow-[0_32px_80px_-15px_rgba(0,0,0,0.2)] overflow-hidden flex flex-col max-h-[85vh]",
        className || "max-w-2xl"
    )}
    onClick={(e) => e.stopPropagation()}
  >
    <div className="px-6 py-4 border-b border-gray-100 flex items-center justify-between bg-gray-50/50 flex-shrink-0">
      <div className="flex items-center gap-3">
        {Icon && <div className="p-2 bg-black text-white rounded-lg shadow-md"><Icon className="w-4 h-4" /></div>}
        <h2 className="text-xl font-black text-gray-900 tracking-tight">{title}</h2>
      </div>
      <button 
        onClick={() => {
            if (onClose) onClose();
            else useStore.getState().setActiveModal(null);
        }}
        className="p-2 rounded-full hover:bg-gray-100 text-gray-400 hover:text-black transition-all active:scale-90"
      >
        <Icons.Close className="w-5 h-5" />
      </button>
    </div>
    <div className="flex-1 overflow-hidden">
      {children}
    </div>
  </motion.div>
);

// Expanded High-fidelity Avatar List
const CHARACTER_AVATARS = [
  `https://api.dicebear.com/9.x/lorelei/svg?seed=Jasper`,
  `https://api.dicebear.com/9.x/lorelei/svg?seed=Mika`,
  `https://api.dicebear.com/9.x/lorelei/svg?seed=Hiro`,
  `https://api.dicebear.com/9.x/lorelei/svg?seed=Yuki`,
  `https://api.dicebear.com/9.x/lorelei/svg?seed=Ren`,
  `https://api.dicebear.com/9.x/lorelei/svg?seed=Sasha`,
  `https://api.dicebear.com/9.x/lorelei/svg?seed=Klaus`,
  `https://api.dicebear.com/9.x/lorelei/svg?seed=Luna`,
  `https://api.dicebear.com/9.x/lorelei/svg?seed=Aneka`,
  `https://api.dicebear.com/9.x/lorelei/svg?seed=Felix`,
  `https://api.dicebear.com/9.x/lorelei/svg?seed=Zoe`,
  `https://api.dicebear.com/9.x/lorelei/svg?seed=Leo`,
  `https://api.dicebear.com/9.x/lorelei/svg?seed=Nora`,
  `https://api.dicebear.com/9.x/lorelei/svg?seed=Caleb`,
  `https://api.dicebear.com/9.x/lorelei/svg?seed=Maya`,
  `https://api.dicebear.com/9.x/lorelei/svg?seed=Oliver`,
  `https://api.dicebear.com/9.x/lorelei/svg?seed=Ruby`,
  `https://api.dicebear.com/9.x/lorelei/svg?seed=Jack`,
  `https://api.dicebear.com/9.x/lorelei/svg?seed=Chloe`,
  `https://api.dicebear.com/9.x/lorelei/svg?seed=Finn`,
];

const PROJECT_COLORS = [
    'bg-blue-500', 'bg-emerald-500', 'bg-violet-500', 'bg-amber-500',
    'bg-rose-500', 'bg-cyan-500', 'bg-indigo-500', 'bg-teal-500',
    'bg-orange-500', 'bg-fuchsia-500', 'bg-slate-500'
];

export const UserModals: React.FC = () => {
  const activeModal = useStore(s => s.activeModal);
  const setActiveModal = useStore(s => s.setActiveModal);
  const language = useStore(s => s.language);
  const setLanguage = useStore(s => s.setLanguage);
  const user = useStore(s => s.user);
  const updateUser = useStore(s => s.updateUser);
  const settings = useStore(s => s.settings);
  const updateSettings = useStore(s => s.updateSettings);
  const editingProjectId = useStore(s => s.editingProjectId);
  const groups = useStore(s => s.groups);
  const updateGroup = useStore(s => s.updateGroup);
  const t = translations[language];

  // Dynamic Skills Data with Localization
  const AVAILABLE_SKILLS = [
    { id: 'web_browsing', name: t.skill_web_browsing_name, description: t.skill_web_browsing_desc, icon: Globe, beta: false, pro: false },
    { id: 'python_interpreter', name: t.skill_python_interpreter_name, description: t.skill_python_interpreter_desc, icon: Code2, beta: true, pro: true },
    { id: 'dalle_3', name: t.skill_dalle_3_name, description: t.skill_dalle_3_desc, icon: ImageIcon, beta: false, pro: true },
    { id: 'memory_core', name: t.skill_memory_core_name, description: t.skill_memory_core_desc, icon: Database, beta: true, pro: false },
    { id: 'code_analysis', name: t.skill_code_analysis_name, description: t.skill_code_analysis_desc, icon: Cpu, beta: false, pro: true },
  ];

  // Local state for forms
  const [displayName, setDisplayName] = useState('');
  const [bio, setBio] = useState('');
  const [helpMessage, setHelpMessage] = useState('');
  const [isSent, setIsSent] = useState(false);

  // Settings Tab State
  const [settingsTab, setSettingsTab] = useState<'general' | 'skills' | 'model' | 'about'>('general');
  // Local state for skills (simulated persistence)
  const [enabledSkills, setEnabledSkills] = useState<Record<string, boolean>>({
      'web_browsing': true,
      'memory_core': true
  });
  // Local state for Model config
  const [temperature, setTemperature] = useState(0.7);
  const [systemPrompt, setSystemPrompt] = useState(t.systemInstruction);

  // Project Settings Local State
  const [projectTitle, setProjectTitle] = useState('');
  const [projectMarker, setProjectMarker] = useState('');
  const [projectColor, setProjectColor] = useState('');

  useEffect(() => {
    if (activeModal === 'account') {
        setDisplayName(user?.name || '');
        setBio(user?.bio || '');
    }
    if (activeModal === 'help') {
        setIsSent(false);
        setHelpMessage('');
    }
    if (activeModal === 'project_edit' && editingProjectId) {
        const group = groups.find(g => g.id === editingProjectId);
        if (group) {
            setProjectTitle(group.title);
            setProjectMarker(group.marker);
            setProjectColor(group.color);
        }
    }
    // Reset Settings tab when opening
    if (activeModal === 'settings') {
        setSettingsTab('general');
    }
  }, [activeModal, user, editingProjectId, groups]);

  const handleSaveAccount = () => {
    updateUser({ name: displayName, bio });
    setActiveModal(null);
  };

  const handleSaveProject = () => {
      if (editingProjectId) {
          updateGroup(editingProjectId, {
              title: projectTitle,
              marker: projectMarker,
              color: projectColor
          });
          setActiveModal(null);
      }
  };

  const handleClearData = () => {
      if (confirm(t.confirmDelete)) {
          localStorage.clear();
          window.location.reload();
      }
  };

  const handleSendHelp = () => {
      setIsSent(true);
      setTimeout(() => setActiveModal(null), 2000);
  };

  const toggleSkill = (id: string) => {
      setEnabledSkills(prev => ({
          ...prev,
          [id]: !prev[id]
      }));
  };

  const settingsTabs = [
      { id: 'general', label: t.general, icon: Icons.Settings },
      { id: 'skills', label: t.skills, icon: Sparkles },
      { id: 'model', label: t.modelConfig, icon: Cpu },
      { id: 'about', label: t.about, icon: Icons.Help },
  ];

  if (!activeModal) return null;

  return (
    <AnimatePresence>
      <ModalBackdrop onClose={() => setActiveModal(null)}>
        {activeModal === 'upgrade' && (
          <ModalContent title={t.upgradeSubscription} icon={Icons.CreditCard}>
            <div className="p-6 overflow-y-auto custom-scrollbar h-full text-center">
              <div className="w-16 h-16 bg-gradient-to-tr from-yellow-300 to-orange-500 rounded-2xl flex items-center justify-center mx-auto mb-4 shadow-2xl shadow-yellow-100">
                <Icons.Zap className="w-8 h-8 text-white" fill="currentColor" />
              </div>
              <h3 className="text-xl font-black mb-1.5 text-gray-900 tracking-tight">{t.unleashPotential}</h3>
              <p className="text-xs text-gray-500 mb-8 font-medium px-4">{t.getAccessPro}</p>
              
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6 px-2 text-left">
                <div className="p-6 rounded-[24px] border border-gray-100 bg-gray-50/30 flex flex-col items-center text-center">
                  <div className="text-[10px] font-black text-gray-400 uppercase tracking-widest mb-1">{t.freePlan}</div>
                  <div className="text-3xl font-black mb-6 text-gray-900">$0</div>
                  <ul className="text-xs space-y-3 text-gray-600 mb-8 w-full font-bold">
                    <li className="flex items-center gap-2.5"><Icons.Check className="w-3.5 h-3.5 text-gray-300" /> {t.standardSpeed}</li>
                    <li className="flex items-center gap-2.5"><Icons.Check className="w-3.5 h-3.5 text-gray-300" /> {t.dailyLimits}</li>
                    <li className="flex items-center gap-2.5"><Icons.Check className="w-3.5 h-3.5 text-gray-300" /> {t.accessKimi}</li>
                  </ul>
                  <button className="w-full py-3 bg-gray-200 text-gray-500 rounded-xl font-black text-xs uppercase cursor-default">Current Plan</button>
                </div>

                <div className="p-6 rounded-[32px] border-4 border-black relative bg-white shadow-2xl shadow-black/10 flex flex-col items-center text-center scale-105 transform">
                  <div className="absolute top-0 right-8 -translate-y-1/2 bg-black text-white text-[9px] font-black px-3 py-1 rounded-full tracking-widest">{t.recommended}</div>
                  <div className="text-[10px] font-black text-gray-900 uppercase tracking-widest mb-1">{t.proPlanCard}</div>
                  <div className="text-3xl font-black mb-6 text-gray-900">$20<span className="text-xs text-gray-400 font-bold">{t.perMonth}</span></div>
                  <ul className="text-xs space-y-3 text-gray-800 mb-8 w-full font-bold">
                    <li className="flex items-center gap-2.5 text-yellow-600"><Icons.Zap className="w-3.5 h-3.5 fill-yellow-400" /> {t.fastSpeed}</li>
                    <li className="flex items-center gap-2.5"><Icons.Check className="w-3.5 h-3.5 text-green-500" /> {t.unlimitedChats}</li>
                    <li className="flex items-center gap-2.5"><Icons.Check className="w-3.5 h-3.5 text-green-500" /> {t.accessDeepseek}</li>
                    <li className="flex items-center gap-2.5"><Icons.Check className="w-3.5 h-3.5 text-green-500" /> {t.prioritySupport}</li>
                  </ul>
                  <button className="w-full py-3 bg-black text-white rounded-xl font-black text-xs hover:bg-gray-800 transition-all shadow-xl active:scale-95 tracking-widest uppercase">{t.upgradeBtn}</button>
                </div>
              </div>
              <p className="text-[9px] text-gray-400 mt-10 flex items-center justify-center gap-2 font-bold opacity-60 uppercase tracking-wide">
                <Icons.CreditCard className="w-3.5 h-3.5" />
                {t.securePayment}
              </p>
            </div>
          </ModalContent>
        )}

        {activeModal === 'account' && (
          <ModalContent title={t.account} icon={Icons.User}>
            <div className="p-6 overflow-y-auto custom-scrollbar h-full space-y-8">
              <div className="space-y-4">
                <div className="flex items-center justify-between">
                  <label className="text-[10px] font-black text-gray-400 uppercase tracking-widest">{t.changeAvatar}</label>
                </div>
                
                <div className="space-y-4">
                    {/* Changed to 2-row layout using grid with horizontal flow */}
                    <div className="grid grid-rows-2 grid-flow-col gap-4 overflow-x-auto pb-4 pt-2 px-2 custom-scrollbar-h snap-x auto-cols-max">
                        {CHARACTER_AVATARS.map((url, i) => (
                            <button 
                                key={i} 
                                onClick={() => updateUser({ avatar: url })}
                                className={clsx(
                                    "relative w-20 h-20 rounded-2xl overflow-hidden border-2 transition-all flex-shrink-0 bg-gray-50 snap-start",
                                    user?.avatar === url 
                                      ? "border-black scale-105 shadow-xl ring-4 ring-black/5" 
                                      : "border-transparent hover:border-gray-200 hover:scale-105"
                                )}
                            >
                                <img src={url} className="w-full h-full object-cover" loading="lazy" />
                                {user?.avatar === url && (
                                  <div className="absolute inset-0 bg-black/10 flex items-center justify-center">
                                    <Icons.Check className="w-8 h-8 text-white drop-shadow-md" />
                                  </div>
                                )}
                            </button>
                        ))}
                    </div>
                </div>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div>
                  <label className="block text-[10px] font-black text-gray-400 uppercase tracking-widest mb-2">{t.displayName}</label>
                  <input 
                    type="text" 
                    value={displayName}
                    onChange={(e) => setDisplayName(e.target.value)}
                    className="w-full px-4 py-3 bg-gray-50 border border-gray-100 rounded-xl outline-none focus:border-black transition-all font-bold text-sm text-gray-900" 
                    placeholder="Your Name" 
                  />
                </div>
                <div>
                  <label className="block text-[10px] font-black text-gray-400 uppercase tracking-widest mb-2">{t.emailLabel}</label>
                  <input 
                    type="email" 
                    disabled
                    className="w-full px-4 py-3 bg-gray-100 border border-transparent rounded-xl text-gray-400 cursor-not-allowed font-bold text-sm" 
                    defaultValue={user?.email} 
                  />
                </div>
              </div>

              <div>
                <label className="block text-[10px] font-black text-gray-400 uppercase tracking-widest mb-2">{t.bio}</label>
                <textarea 
                    value={bio}
                    onChange={(e) => setBio(e.target.value)}
                    className="w-full px-4 py-3 bg-gray-50 border border-gray-100 rounded-xl outline-none focus:border-black transition-all font-bold text-sm text-gray-900 h-24 resize-none" 
                    placeholder={t.bioPlaceholder} 
                />
              </div>

              <div className="pt-5 border-t border-gray-100 flex justify-end gap-3">
                 <button 
                    onClick={() => setActiveModal(null)}
                    className="px-6 py-3 text-gray-500 font-black rounded-xl hover:bg-gray-100 transition-all uppercase tracking-widest text-[11px]"
                 >
                    {t.cancel}
                 </button>
                 <button 
                    onClick={handleSaveAccount}
                    className="px-8 py-3 bg-black text-white font-black rounded-xl hover:bg-gray-800 transition-all shadow-md active:scale-95 uppercase tracking-widest text-[11px]"
                 >
                    {t.saveChanges}
                 </button>
              </div>
            </div>
          </ModalContent>
        )}

        {activeModal === 'settings' && (
          <ModalContent title={t.settings} icon={Icons.Settings} className="max-w-4xl h-[600px]">
            <div className="flex h-full">
                {/* Left Sidebar Tabs */}
                <div className="w-56 bg-gray-50 border-r border-gray-100 p-4 space-y-1">
                    <div className="text-[10px] font-black text-gray-400 uppercase tracking-widest mb-3 px-3">Categories</div>
                    
                    {settingsTabs.map(tab => (
                        <button
                            key={tab.id}
                            onClick={() => setSettingsTab(tab.id as any)}
                            className={clsx(
                                "w-full flex items-center gap-3 px-4 py-3 rounded-xl transition-all text-sm font-bold",
                                settingsTab === tab.id 
                                    ? "bg-black text-white shadow-md" 
                                    : "text-gray-500 hover:bg-white hover:text-black"
                            )}
                        >
                            <tab.icon className="w-4 h-4" />
                            <span>{tab.label}</span>
                        </button>
                    ))}
                </div>

                {/* Right Content Area */}
                <div className="flex-1 overflow-y-auto custom-scrollbar p-8 bg-white">
                    {settingsTab === 'general' && (
                        <div className="space-y-8 animate-fadeIn">
                             <section className="space-y-4">
                                <h3 className="text-[10px] font-black text-gray-400 uppercase tracking-widest">{t.languageName}</h3>
                                <div className="grid grid-cols-2 gap-4">
                                    <button 
                                        onClick={() => setLanguage('en')}
                                        className={clsx(
                                            "flex items-center justify-center gap-2 py-3 rounded-xl border-2 transition-all font-bold text-sm",
                                            language === 'en' ? "border-black bg-black text-white" : "border-gray-100 text-gray-600 hover:border-gray-300"
                                        )}
                                    >
                                        <span className="text-lg">🇺🇸</span> English
                                    </button>
                                    <button 
                                        onClick={() => setLanguage('zh')}
                                        className={clsx(
                                            "flex items-center justify-center gap-2 py-3 rounded-xl border-2 transition-all font-bold text-sm",
                                            language === 'zh' ? "border-black bg-black text-white" : "border-gray-100 text-gray-600 hover:border-gray-300"
                                        )}
                                    >
                                        <span className="text-lg">🇨🇳</span> 简体中文
                                    </button>
                                </div>
                            </section>
                            
                            <hr className="border-gray-100" />

                            <section className="space-y-4">
                                <h3 className="text-[10px] font-black text-gray-400 uppercase tracking-widest">Preferences</h3>
                                <div className="space-y-3">
                                    <label className="flex items-center justify-between p-4 bg-white border border-gray-100 rounded-2xl hover:border-gray-300 transition-all cursor-pointer">
                                        <div className="flex items-center gap-4">
                                            <div className="p-2 bg-blue-50 text-blue-600 rounded-lg">
                                                <Icons.Zap className="w-5 h-5" />
                                            </div>
                                            <div>
                                                <div className="font-bold text-gray-900">{t.streamResponses}</div>
                                                <div className="text-xs text-gray-500 font-medium">{t.streamDesc}</div>
                                            </div>
                                        </div>
                                        <div className={clsx("w-12 h-6 rounded-full relative transition-all duration-300", settings.streamResponses ? "bg-black" : "bg-gray-200")}>
                                            <input 
                                                type="checkbox" 
                                                className="hidden" 
                                                checked={settings.streamResponses} 
                                                onChange={(e) => updateSettings({ streamResponses: e.target.checked })}
                                            />
                                            <motion.div animate={{ x: settings.streamResponses ? 24 : 4 }} className="w-4 h-4 bg-white rounded-full shadow-md absolute top-1" />
                                        </div>
                                    </label>

                                    <label className="flex items-center justify-between p-4 bg-white border border-gray-100 rounded-2xl hover:border-gray-300 transition-all cursor-pointer">
                                        <div className="flex items-center gap-4">
                                            <div className="p-2 bg-purple-50 text-purple-600 rounded-lg">
                                                <Volume2 className="w-5 h-5" />
                                            </div>
                                            <div>
                                                <div className="font-bold text-gray-900">{t.soundEffects}</div>
                                                <div className="text-xs text-gray-500 font-medium">{t.soundDesc}</div>
                                            </div>
                                        </div>
                                        <div className={clsx("w-12 h-6 rounded-full relative transition-all duration-300", settings.soundEffects ? "bg-black" : "bg-gray-200")}>
                                            <input 
                                                type="checkbox" 
                                                className="hidden" 
                                                checked={settings.soundEffects} 
                                                onChange={(e) => updateSettings({ soundEffects: e.target.checked })}
                                            />
                                            <motion.div animate={{ x: settings.soundEffects ? 24 : 4 }} className="w-4 h-4 bg-white rounded-full shadow-md absolute top-1" />
                                        </div>
                                    </label>
                                </div>
                            </section>

                            <hr className="border-gray-100" />

                            <section className="space-y-4">
                                <h3 className="text-[10px] font-black text-gray-400 uppercase tracking-widest">{t.dataPrivacy}</h3>
                                <button 
                                    onClick={handleClearData}
                                    className="w-full flex items-center justify-between p-4 bg-red-50 text-red-600 rounded-2xl hover:bg-red-100 transition-all border border-red-100 group"
                                >
                                    <div className="flex items-center gap-4">
                                        <ShieldAlert className="w-5 h-5" />
                                        <span className="font-bold text-sm">{t.clearData}</span>
                                    </div>
                                    <Icons.Trash className="w-4 h-4 group-hover:rotate-12 transition-transform" />
                                </button>
                            </section>
                        </div>
                    )}

                    {settingsTab === 'skills' && (
                        <div className="space-y-6 animate-fadeIn">
                             <div className="flex items-center justify-between mb-2">
                                 <div>
                                    <h3 className="text-lg font-black text-gray-900">{t.agentCapabilities}</h3>
                                    <p className="text-xs text-gray-500 font-medium">{t.agentCapabilitiesDesc}</p>
                                 </div>
                                 <div className="px-3 py-1 bg-blue-50 text-blue-600 rounded-full text-[10px] font-black uppercase tracking-wider">
                                     Beta
                                 </div>
                             </div>

                             <div className="grid grid-cols-1 gap-4">
                                {AVAILABLE_SKILLS.map(skill => (
                                    <div key={skill.id} className="flex items-start justify-between p-4 bg-white border border-gray-200 rounded-2xl hover:shadow-md transition-shadow group">
                                        <div className="flex gap-4">
                                            <div className="p-3 bg-gray-50 rounded-xl group-hover:bg-black group-hover:text-white transition-colors">
                                                <skill.icon className="w-5 h-5" />
                                            </div>
                                            <div>
                                                <div className="flex items-center gap-2 mb-1">
                                                    <span className="font-bold text-gray-900 text-sm">{skill.name}</span>
                                                    {skill.beta && <span className="px-1.5 py-0.5 bg-yellow-100 text-yellow-700 text-[9px] font-bold rounded uppercase">Beta</span>}
                                                    {skill.pro && <span className="px-1.5 py-0.5 bg-black text-white text-[9px] font-bold rounded uppercase">Pro</span>}
                                                </div>
                                                <p className="text-xs text-gray-500 font-medium leading-relaxed max-w-[280px]">{skill.description}</p>
                                            </div>
                                        </div>
                                        <button 
                                            onClick={() => toggleSkill(skill.id)}
                                            className={clsx(
                                                "w-12 h-6 rounded-full relative transition-all duration-300 flex-shrink-0 mt-1", 
                                                enabledSkills[skill.id] ? "bg-black" : "bg-gray-200"
                                            )}
                                        >
                                            <motion.div animate={{ x: enabledSkills[skill.id] ? 24 : 4 }} className="w-4 h-4 bg-white rounded-full shadow-md absolute top-1" />
                                        </button>
                                    </div>
                                ))}
                             </div>

                             {/* Drag and Drop Upload Area */}
                             <div className="mt-6 border-2 border-dashed border-gray-200 rounded-2xl p-6 flex flex-col items-center justify-center text-center hover:border-black/20 transition-colors cursor-pointer bg-gray-50/50 group">
                                <div className="w-12 h-12 bg-white rounded-full flex items-center justify-center mb-3 text-gray-400 shadow-sm border border-gray-100 group-hover:scale-105 transition-transform">
                                    <Upload className="w-5 h-5" />
                                </div>
                                <div className="text-sm font-bold text-gray-900">{t.importSkill}</div>
                                <div className="text-xs text-gray-500 mt-1 mb-3">{t.dropSkillHere}</div>
                                <button className="px-4 py-1.5 bg-white border border-gray-200 rounded-lg text-xs font-bold text-gray-700 hover:bg-gray-50 transition-colors shadow-sm">
                                    {t.browse}
                                </button>
                             </div>
                        </div>
                    )}

                    {settingsTab === 'model' && (
                         <div className="space-y-8 animate-fadeIn">
                            <section className="space-y-4">
                                <div>
                                    <h3 className="text-[10px] font-black text-gray-400 uppercase tracking-widest mb-1">{t.systemInstructions}</h3>
                                    <p className="text-xs text-gray-500 font-medium mb-3">{t.systemInstructionsDesc}</p>
                                </div>
                                <textarea 
                                    value={systemPrompt}
                                    onChange={(e) => setSystemPrompt(e.target.value)}
                                    className="w-full h-40 p-4 bg-gray-50 border border-gray-200 rounded-2xl text-sm font-medium text-gray-800 outline-none focus:border-black focus:ring-1 focus:ring-black transition-all resize-none"
                                />
                            </section>

                            <section className="space-y-6">
                                <div>
                                    <div className="flex items-center justify-between mb-2">
                                        <h3 className="text-[10px] font-black text-gray-400 uppercase tracking-widest">{t.creativity}</h3>
                                        <span className="text-xs font-bold text-gray-900 bg-gray-100 px-2 py-0.5 rounded-md">{temperature}</span>
                                    </div>
                                    <input 
                                        type="range" 
                                        min="0" 
                                        max="1" 
                                        step="0.1" 
                                        value={temperature}
                                        onChange={(e) => setTemperature(parseFloat(e.target.value))}
                                        className="w-full h-2 bg-gray-200 rounded-lg appearance-none cursor-pointer accent-black"
                                    />
                                    <div className="flex justify-between text-[10px] font-bold text-gray-400 mt-2">
                                        <span>{t.precise}</span>
                                        <span>{t.balanced}</span>
                                        <span>{t.creative}</span>
                                    </div>
                                </div>
                            </section>

                            <div className="p-4 bg-blue-50 rounded-2xl border border-blue-100 flex gap-3">
                                <Icons.Zap className="w-5 h-5 text-blue-600 flex-shrink-0" />
                                <div>
                                    <h4 className="text-xs font-bold text-blue-800 mb-1">{t.modelAvailability}</h4>
                                    <p className="text-[11px] text-blue-600 leading-relaxed">
                                        {t.modelAvailabilityDesc}
                                    </p>
                                </div>
                            </div>
                         </div>
                    )}

                    {settingsTab === 'about' && (
                         <div className="flex flex-col items-center justify-center h-full animate-fadeIn text-center space-y-6">
                            <div className="w-20 h-20 bg-black text-white rounded-3xl flex items-center justify-center shadow-2xl">
                                <Icons.Zap className="w-10 h-10" fill="currentColor" />
                            </div>
                            <div>
                                <h2 className="text-2xl font-black text-gray-900 tracking-tighter">Agent UI</h2>
                                <p className="text-sm text-gray-500 font-medium">{t.version} 1.2.0 (Build 2025.05.15)</p>
                            </div>
                            <div className="flex gap-4">
                                <a href="#" className="px-4 py-2 bg-gray-100 rounded-lg text-xs font-bold text-gray-600 hover:bg-gray-200 hover:text-black transition-colors">{t.changelog}</a>
                                <a href="#" className="px-4 py-2 bg-gray-100 rounded-lg text-xs font-bold text-gray-600 hover:bg-gray-200 hover:text-black transition-colors">{t.privacyPolicy}</a>
                            </div>
                            <p className="text-[10px] text-gray-400 max-w-xs leading-relaxed">
                                {t.aboutDesc}
                            </p>
                         </div>
                    )}
                </div>
            </div>
          </ModalContent>
        )}

        {activeModal === 'help' && (
          <ModalContent title={t.getHelp} icon={Icons.Help}>
            <div className="p-6 overflow-y-auto custom-scrollbar h-full space-y-8">
              <div className="p-6 bg-blue-50 rounded-3xl border border-blue-100 relative overflow-hidden group">
                <div className="relative z-10">
                    <h3 className="text-lg font-black text-blue-900 mb-1">{t.needHelpTitle}</h3>
                    <p className="text-xs text-blue-700 mb-5 font-medium leading-relaxed">{t.needHelpDesc}</p>
                    <button className="flex items-center gap-2.5 px-5 py-2.5 bg-blue-600 text-white rounded-xl text-[11px] font-black hover:bg-blue-700 transition-all shadow-md uppercase tracking-widest">
                        <Icons.Mail className="w-3.5 h-3.5" />
                        {t.contactSupport}
                    </button>
                </div>
                <Icons.Sparkles className="absolute -bottom-4 -right-4 w-24 h-24 text-blue-100 opacity-40 group-hover:scale-110 transition-transform duration-700" />
              </div>

              <div className="space-y-6">
                <h3 className="text-[10px] font-black text-gray-400 uppercase tracking-widest">{t.recent} FAQ</h3>
                <div className="space-y-5">
                  <div className="space-y-1">
                    <h4 className="font-black text-gray-900 text-base">{t.faq1Title}</h4>
                    <p className="text-xs text-gray-500 leading-relaxed font-medium">{t.faq1Desc}</p>
                  </div>
                  <div className="space-y-1">
                    <h4 className="font-black text-gray-900 text-base">{t.faq2Title}</h4>
                    <p className="text-xs text-gray-500 leading-relaxed font-medium">{t.faq2Desc}</p>
                  </div>
                </div>
              </div>

              <div className="pt-6 border-t border-gray-100">
                <label className="block text-[10px] font-black text-gray-400 uppercase tracking-widest mb-3">{t.describeIssue}</label>
                <textarea 
                    value={helpMessage}
                    onChange={(e) => setHelpMessage(e.target.value)}
                    className="w-full px-4 py-3 bg-gray-50 border border-gray-100 rounded-2xl outline-none focus:border-black transition-all font-bold text-sm text-gray-900 h-24 resize-none mb-5" 
                />
                <button 
                    onClick={handleSendHelp}
                    disabled={!helpMessage.trim() || isSent}
                    className={clsx(
                        "w-full py-4 rounded-2xl font-black transition-all shadow-md active:scale-95 uppercase tracking-widest text-[11px] flex items-center justify-center gap-2.5",
                        isSent ? "bg-green-500 text-white" : "bg-black text-white hover:bg-gray-800 disabled:bg-gray-200 disabled:text-gray-400"
                    )}
                >
                    {isSent ? (
                        <>
                            <Icons.Check className="w-4 h-4" />
                            <span>Message Sent</span>
                        </>
                    ) : (
                        t.sendMessage
                    )}
                </button>
              </div>
            </div>
          </ModalContent>
        )}

        {activeModal === 'project_edit' && (
             <ModalContent title="Project Settings" icon={Icons.Settings}>
                <div className="p-6 overflow-y-auto custom-scrollbar h-full space-y-8">
                   <div>
                       <label className="block text-[10px] font-black text-gray-400 uppercase tracking-widest mb-2.5">Project Title</label>
                       <input 
                           type="text" 
                           value={projectTitle}
                           onChange={(e) => setProjectTitle(e.target.value)}
                           className="w-full px-4 py-3.5 bg-gray-50 border border-gray-100 rounded-xl outline-none focus:border-black transition-all font-bold text-sm text-gray-900" 
                           placeholder="My Awesome Project" 
                       />
                   </div>

                   <div className="grid grid-cols-1 sm:grid-cols-2 gap-8">
                       <div>
                           <label className="block text-[10px] font-black text-gray-400 uppercase tracking-widest mb-2.5">Marker (Char)</label>
                           <div className="flex gap-4 items-center">
                               <input 
                                   type="text" 
                                   maxLength={1}
                                   value={projectMarker}
                                   onChange={(e) => setProjectMarker(e.target.value)}
                                   className="w-14 h-14 text-center text-lg bg-gray-50 border border-gray-100 rounded-xl outline-none focus:border-black transition-all font-black text-gray-900 uppercase" 
                               />
                               <p className="text-[10px] text-gray-400 font-medium leading-relaxed">Appears in sidebar icon.</p>
                           </div>
                       </div>
                   </div>

                   <div>
                       <label className="block text-[10px] font-black text-gray-400 uppercase tracking-widest mb-3.5">Theme Color</label>
                       <div className="grid grid-cols-6 gap-2.5">
                           {PROJECT_COLORS.map((color) => (
                               <button 
                                   key={color}
                                   onClick={() => setProjectColor(color)}
                                   className={clsx(
                                       "w-full aspect-square rounded-lg transition-all relative border-2",
                                       projectColor === color ? "border-black scale-105 shadow-md" : "border-transparent hover:scale-105",
                                       color
                                   )}
                               >
                                   {projectColor === color && <Icons.Check className="w-3.5 h-3.5 text-white absolute inset-0 m-auto" />}
                               </button>
                           ))}
                       </div>
                   </div>

                   <div className="pt-6 border-t border-gray-100 flex gap-3">
                       <button 
                         onClick={() => setActiveModal(null)}
                         className="flex-1 py-3.5 bg-gray-100 text-gray-500 rounded-xl font-black uppercase tracking-widest text-[11px] transition-all hover:bg-gray-200"
                       >
                         {t.cancel}
                       </button>
                       <button 
                         onClick={handleSaveProject}
                         className="flex-1 py-3.5 bg-black text-white rounded-xl font-black uppercase tracking-widest text-[11px] shadow-lg hover:bg-gray-800 transition-all active:scale-95"
                       >
                         {t.saveChanges}
                       </button>
                   </div>
                </div>
             </ModalContent>
          )}

          {activeModal === 'quota_limit' && (
              <ModalContent title={t.quotaTitle} icon={Icons.Close}>
                 <div className="text-center py-6 px-4">
                    <div className="w-16 h-16 bg-red-100 text-red-600 rounded-full flex items-center justify-center mx-auto mb-6 shadow-sm border border-red-50">
                       <Icons.Trash className="w-8 h-8" />
                    </div>
                    <p className="text-gray-600 text-sm font-medium leading-relaxed mb-10">
                       {t.quotaDesc}
                    </p>
                    <div className="flex gap-4">
                        <button 
                            onClick={() => setActiveModal(null)}
                            className="flex-1 py-3.5 bg-gray-100 text-gray-900 rounded-xl font-black uppercase tracking-widest text-[11px] hover:bg-gray-200 transition-all"
                        >
                            {t.understood}
                        </button>
                        <button 
                            onClick={() => setActiveModal('upgrade')}
                            className="flex-1 py-3.5 bg-black text-white rounded-xl font-black uppercase tracking-widest text-[11px] hover:bg-gray-800 transition-all shadow-md active:scale-95"
                        >
                            {t.upgradeSubscription}
                        </button>
                    </div>
                 </div>
              </ModalContent>
          )}
      </ModalBackdrop>
    </AnimatePresence>
  );
};
