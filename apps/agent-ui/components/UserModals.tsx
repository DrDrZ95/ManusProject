
import React, { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { useStore } from '../store';
import { Icons } from './icons';
import clsx from 'clsx';
import { translations } from '../locales';

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

const ModalContent: React.FC<{ children: React.ReactNode; title: string; icon?: any; onClose?: () => void }> = ({ children, title, icon: Icon, onClose }) => (
  <motion.div 
    initial={{ scale: 0.95, opacity: 0, y: 20 }}
    animate={{ scale: 1, opacity: 1, y: 0 }}
    exit={{ scale: 0.95, opacity: 0, y: 20 }}
    transition={{ type: "spring", stiffness: 300, damping: 30 }}
    className="bg-white w-full max-w-2xl rounded-[28px] shadow-[0_32px_80px_-15px_rgba(0,0,0,0.2)] overflow-hidden flex flex-col max-h-[85vh]"
    onClick={(e) => e.stopPropagation()}
  >
    <div className="px-6 py-4 border-b border-gray-100 flex items-center justify-between bg-gray-50/50">
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
    <div className="p-6 overflow-y-auto custom-scrollbar">
      {children}
    </div>
  </motion.div>
);

// High-fidelity Asian Anime characters (Lorelei style)
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

  // Local state for forms
  const [displayName, setDisplayName] = useState('');
  const [bio, setBio] = useState('');
  const [helpMessage, setHelpMessage] = useState('');
  const [isSent, setIsSent] = useState(false);

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

  if (!activeModal) return null;

  return (
    <AnimatePresence>
      <ModalBackdrop onClose={() => setActiveModal(null)}>
        {activeModal === 'upgrade' && (
          <ModalContent title={t.upgradeSubscription} icon={Icons.CreditCard}>
            <div className="text-center">
              <div className="w-16 h-16 bg-gradient-to-tr from-yellow-300 to-orange-500 rounded-2xl flex items-center justify-center mx-auto mb-4 shadow-2xl shadow-yellow-100">
                <Icons.Zap className="w-8 h-8 text-white" fill="currentColor" />
              </div>
              <h3 className="text-xl font-black mb-1.5 text-gray-900 tracking-tight">{t.unleashPotential}</h3>
              <p className="text-xs text-gray-500 mb-8 font-medium px-4">{t.getAccessPro}</p>
              
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6 px-2">
                <div className="p-6 rounded-[24px] border border-gray-100 bg-gray-50/30 flex flex-col items-center">
                  <div className="text-[10px] font-black text-gray-400 uppercase tracking-widest mb-1">{t.freePlan}</div>
                  <div className="text-3xl font-black mb-6 text-gray-900">$0</div>
                  <ul className="text-xs space-y-3 text-gray-600 mb-8 w-full font-bold">
                    <li className="flex items-center gap-2.5"><Icons.Check className="w-3.5 h-3.5 text-gray-300" /> {t.standardSpeed}</li>
                    <li className="flex items-center gap-2.5"><Icons.Check className="w-3.5 h-3.5 text-gray-300" /> {t.dailyLimits}</li>
                    <li className="flex items-center gap-2.5"><Icons.Check className="w-3.5 h-3.5 text-gray-300" /> {t.accessKimi}</li>
                  </ul>
                  <button className="w-full py-3 bg-gray-200 text-gray-500 rounded-xl font-black text-xs uppercase cursor-default">Current Plan</button>
                </div>

                <div className="p-6 rounded-[32px] border-4 border-black relative bg-white shadow-2xl shadow-black/10 flex flex-col items-center scale-105 transform">
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
            <div className="space-y-8">
              <div className="space-y-4">
                <div className="flex items-center justify-between">
                  <label className="text-[10px] font-black text-gray-400 uppercase tracking-widest">{t.changeAvatar}</label>
                  <span className="text-[9px] bg-black text-white px-1.5 py-0.5 rounded font-black">{t.avatarSize}</span>
                </div>
                
                <div className="space-y-4">
                    {/* Added padding px-2 and py-2 to prevent shadow/border clipping */}
                    <div className="flex gap-4 overflow-x-auto pb-4 pt-2 px-2 custom-scrollbar-h">
                        {CHARACTER_AVATARS.map((url, i) => (
                            <button 
                                key={i} 
                                onClick={() => updateUser({ avatar: url })}
                                className={clsx(
                                    "relative w-16 h-16 rounded-2xl overflow-hidden border-2 transition-all flex-shrink-0 bg-gray-50 shadow-sm",
                                    user?.avatar === url ? "border-black scale-110 shadow-lg ring-4 ring-black/5" : "border-transparent hover:border-gray-200"
                                )}
                            >
                                <img src={url} className="w-full h-full object-cover filter grayscale" />
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
          <ModalContent title={t.settings} icon={Icons.Settings}>
            <div className="space-y-10">
              <section className="space-y-4">
                <h3 className="text-[10px] font-black text-gray-400 uppercase tracking-widest">{t.languageName}</h3>
                <div className="flex bg-gray-100 rounded-xl p-1 w-full max-w-xs">
                    <button 
                        onClick={() => setLanguage('en')}
                        className={clsx(
                            "flex-1 py-2 text-xs font-black rounded-lg transition-all",
                            language === 'en' ? "bg-white text-black shadow-sm" : "text-gray-400 hover:text-gray-600"
                        )}
                    >
                        English
                    </button>
                    <button 
                        onClick={() => setLanguage('zh')}
                        className={clsx(
                            "flex-1 py-2 text-xs font-black rounded-lg transition-all",
                            language === 'zh' ? "bg-white text-black shadow-sm" : "text-gray-400 hover:text-gray-600"
                        )}
                    >
                        简体中文
                    </button>
                </div>
              </section>

              <section className="space-y-4">
                <h3 className="text-[10px] font-black text-gray-400 uppercase tracking-widest">{t.general}</h3>
                <div className="space-y-3">
                  <label className="flex items-center justify-between p-4 bg-gray-50 rounded-2xl hover:bg-gray-100 transition-all cursor-pointer border border-transparent hover:border-gray-200 group">
                    <div>
                      <div className="font-black text-gray-900 text-base">{t.streamResponses}</div>
                      <div className="text-[11px] text-gray-500 mt-0.5 font-medium">{t.streamDesc}</div>
                    </div>
                    <div className={clsx("w-12 h-6.5 rounded-full relative transition-all duration-300", settings.streamResponses ? "bg-black" : "bg-gray-200")}>
                        <input 
                            type="checkbox" 
                            className="hidden" 
                            checked={settings.streamResponses} 
                            onChange={(e) => updateSettings({ streamResponses: e.target.checked })}
                        />
                        <motion.div 
                            animate={{ x: settings.streamResponses ? 24 : 4 }}
                            className="w-5 h-5 bg-white rounded-full shadow-md absolute top-0.5" 
                        />
                    </div>
                  </label>
                  
                  <label className="flex items-center justify-between p-4 bg-gray-50 rounded-2xl hover:bg-gray-100 transition-all cursor-pointer border border-transparent hover:border-gray-200 group">
                    <div>
                      <div className="font-black text-gray-900 text-base">{t.soundEffects}</div>
                      <div className="text-[11px] text-gray-500 mt-0.5 font-medium">{t.soundDesc}</div>
                    </div>
                    <div className={clsx("w-12 h-6.5 rounded-full relative transition-all duration-300", settings.soundEffects ? "bg-black" : "bg-gray-200")}>
                        <input 
                            type="checkbox" 
                            className="hidden" 
                            checked={settings.soundEffects} 
                            onChange={(e) => updateSettings({ soundEffects: e.target.checked })}
                        />
                        <motion.div 
                            animate={{ x: settings.soundEffects ? 24 : 4 }}
                            className="w-5 h-5 bg-white rounded-full shadow-md absolute top-0.5" 
                        />
                    </div>
                  </label>
                </div>
              </section>

              <section className="space-y-4">
                <h3 className="text-[10px] font-black text-gray-400 uppercase tracking-widest">{t.dataPrivacy}</h3>
                <div className="space-y-3">
                  <label className="flex items-center justify-between p-4 bg-gray-50 rounded-2xl hover:bg-gray-100 transition-all cursor-pointer border border-transparent hover:border-gray-200 group">
                    <div>
                      <div className="font-black text-gray-900 text-base">{t.trainingData}</div>
                      <div className="text-[11px] text-gray-500 mt-0.5 font-medium">{t.trainingDesc}</div>
                    </div>
                    <div className={clsx("w-12 h-6.5 rounded-full relative transition-all duration-300", settings.allowTraining ? "bg-black" : "bg-gray-200")}>
                        <input 
                            type="checkbox" 
                            className="hidden" 
                            checked={settings.allowTraining} 
                            onChange={(e) => updateSettings({ allowTraining: e.target.checked })}
                        />
                        <motion.div 
                            animate={{ x: settings.allowTraining ? 24 : 4 }}
                            className="w-5 h-5 bg-white rounded-full shadow-md absolute top-0.5" 
                        />
                    </div>
                  </label>
                  <button 
                    onClick={handleClearData}
                    className="w-full flex items-center justify-between p-4 bg-red-50 text-red-600 rounded-2xl hover:bg-red-100 transition-all border border-red-100 group"
                  >
                    <span className="font-black text-base">{t.clearData}</span>
                    <Icons.Trash className="w-4 h-4 group-hover:rotate-6 transition-transform" />
                  </button>
                </div>
              </section>

              <button 
                onClick={() => setActiveModal(null)}
                className="w-full py-4 bg-black text-white font-black rounded-2xl hover:bg-gray-800 transition-all shadow-lg uppercase tracking-widest text-[11px] active:scale-[0.98]"
              >
                {t.done}
              </button>
            </div>
          </ModalContent>
        )}

        {activeModal === 'help' && (
          <ModalContent title={t.getHelp} icon={Icons.Help}>
            <div className="space-y-8">
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
                <div className="space-y-8">
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
              // Fix: Changed Icons.X to Icons.Close as 'X' is not a property of Icons. Icons.Close maps to 'X' from lucide-react.
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
