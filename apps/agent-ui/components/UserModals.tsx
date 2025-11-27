
import React, { useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { useStore } from '../store';
import { Icons } from './icons';
import clsx from 'clsx';
import { translations } from '../locales';

const ModalBackdrop = ({ children, onClose }: { children: React.ReactNode; onClose: () => void }) => (
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

const ModalContent = ({ children, title, icon: Icon }: { children: React.ReactNode; title: string; icon?: any }) => (
  <motion.div 
    initial={{ scale: 0.95, opacity: 0, y: 20 }}
    animate={{ scale: 1, opacity: 1, y: 0 }}
    exit={{ scale: 0.95, opacity: 0, y: 20 }}
    transition={{ type: "spring", duration: 0.5 }}
    className="bg-white w-full max-w-2xl rounded-2xl shadow-2xl overflow-hidden flex flex-col max-h-[85vh]"
    onClick={(e) => e.stopPropagation()}
  >
    <div className="px-6 py-4 border-b border-gray-100 flex items-center justify-between bg-gray-50/50">
      <div className="flex items-center gap-3">
        {Icon && <div className="p-2 bg-gray-100 rounded-lg"><Icon className="w-5 h-5 text-gray-700" /></div>}
        <h2 className="text-xl font-bold text-gray-900 tracking-tight">{title}</h2>
      </div>
      <button 
        onClick={() => useStore.getState().setActiveModal(null)}
        className="p-2 rounded-full hover:bg-gray-100 text-gray-400 hover:text-gray-600 transition-colors"
      >
        <Icons.Close className="w-5 h-5" />
      </button>
    </div>
    <div className="p-6 overflow-y-auto custom-scrollbar">
      {children}
    </div>
  </motion.div>
);

// Optimized Anime & Cartoon Style Avatars
const CARTOON_AVATARS = [
  'https://api.dicebear.com/9.x/adventurer/svg?seed=Felix',
  'https://api.dicebear.com/9.x/adventurer/svg?seed=Coco',
  'https://api.dicebear.com/9.x/notionists/svg?seed=Bear',
  'https://api.dicebear.com/9.x/notionists/svg?seed=Cookie',
  'https://api.dicebear.com/9.x/avataaars/svg?seed=Zoe',
  'https://api.dicebear.com/9.x/micah/svg?seed=Leo',
  'https://api.dicebear.com/9.x/micah/svg?seed=Callie',
  'https://api.dicebear.com/9.x/big-smile/svg?seed=Happy',
  'https://api.dicebear.com/9.x/bottts/svg?seed=Robot',
  'https://api.dicebear.com/9.x/fun-emoji/svg?seed=Spooky',
  // Anime-ish styles using other collections or seeds
  'https://api.dicebear.com/9.x/adventurer/svg?seed=Saitama', 
  'https://api.dicebear.com/9.x/micah/svg?seed=Nezuko' 
];

export const UserModals: React.FC = () => {
  const activeModal = useStore(s => s.activeModal);
  const setActiveModal = useStore(s => s.setActiveModal);
  const lang = useStore(s => s.language);
  const user = useStore(s => s.user);
  const updateUser = useStore(s => s.updateUser);
  const settings = useStore(s => s.settings);
  const updateSettings = useStore(s => s.updateSettings);

  const t = translations[lang];
  const [isSelectingAvatar, setIsSelectingAvatar] = useState(false);
  const [tempBio, setTempBio] = useState(user?.bio || '');
  const [tempName, setTempName] = useState(user?.name || 'Agent User');

  const onClose = () => {
      setIsSelectingAvatar(false);
      setActiveModal(null);
  };

  const handleSaveProfile = () => {
      updateUser({ name: tempName, bio: tempBio });
      onClose();
  };

  if (!activeModal) return null;

  return (
    <AnimatePresence>
      {activeModal && (
        <ModalBackdrop onClose={onClose}>
          {activeModal === 'upgrade' && (
            <ModalContent title={t.upgradeSubscription} icon={Icons.Zap}>
              <div className="space-y-6">
                <div className="text-center mb-8">
                  <h3 className="text-2xl font-bold mb-2">{t.unleashPotential}</h3>
                  <p className="text-gray-500">{t.getAccessPro}</p>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div className="border border-gray-200 rounded-xl p-6 hover:border-blue-500 transition-colors cursor-pointer relative">
                    <div className="text-lg font-semibold mb-2">{t.freePlan}</div>
                    <div className="text-3xl font-bold mb-4">$0<span className="text-sm text-gray-400 font-normal">{t.perMonth}</span></div>
                    <ul className="space-y-3 text-sm text-gray-600">
                      <li className="flex items-center gap-2"><Icons.Check className="w-4 h-4 text-green-500" /> {t.standardSpeed}</li>
                      <li className="flex items-center gap-2"><Icons.Check className="w-4 h-4 text-green-500" /> {t.dailyLimits}</li>
                      <li className="flex items-center gap-2"><Icons.Check className="w-4 h-4 text-green-500" /> {t.accessKimi}</li>
                    </ul>
                  </div>

                  <div className="border-2 border-black rounded-xl p-6 relative shadow-lg bg-gray-50">
                    <div className="absolute top-0 right-0 bg-black text-white text-xs font-bold px-3 py-1 rounded-bl-xl rounded-tr-lg">{t.recommended}</div>
                    <div className="text-lg font-semibold mb-2">{t.proPlanCard}</div>
                    <div className="text-3xl font-bold mb-4">$20<span className="text-sm text-gray-400 font-normal">{t.perMonth}</span></div>
                    <ul className="space-y-3 text-sm text-gray-600">
                      <li className="flex items-center gap-2"><Icons.Check className="w-4 h-4 text-black" /> {t.fastSpeed}</li>
                      <li className="flex items-center gap-2"><Icons.Check className="w-4 h-4 text-black" /> {t.unlimitedChats}</li>
                      <li className="flex items-center gap-2"><Icons.Check className="w-4 h-4 text-black" /> {t.accessDeepseek}</li>
                      <li className="flex items-center gap-2"><Icons.Check className="w-4 h-4 text-black" /> {t.prioritySupport}</li>
                    </ul>
                  </div>
                </div>

                <div className="mt-8 pt-6 border-t border-gray-100">
                  <button className="w-full bg-black text-white py-4 rounded-xl font-bold text-lg hover:bg-gray-800 transition-all shadow-lg transform hover:scale-[1.01]">
                    {t.upgradeBtn}
                  </button>
                  <p className="text-center text-xs text-gray-400 mt-4">{t.securePayment}</p>
                </div>
              </div>
            </ModalContent>
          )}

          {activeModal === 'account' && (
            <ModalContent title={t.account} icon={Icons.User}>
              <div className="space-y-6">
                <div className="flex items-center gap-6 pb-6 border-b border-gray-100">
                  <div className="relative group cursor-pointer" onClick={() => setIsSelectingAvatar(!isSelectingAvatar)}>
                    <motion.div 
                      layoutId="current-avatar"
                      className="w-24 h-24 rounded-full bg-gray-100 flex items-center justify-center overflow-hidden shadow-sm border-4 border-white ring-1 ring-gray-200"
                    >
                       <img src={user?.avatar} alt="Avatar" className="w-full h-full object-cover" />
                    </motion.div>
                    <div className="absolute inset-0 bg-black/40 rounded-full flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity">
                        <Icons.Edit className="w-6 h-6 text-white" />
                    </div>
                  </div>
                  <div className="flex-1">
                    <button onClick={() => setIsSelectingAvatar(!isSelectingAvatar)} className="text-base font-semibold text-black hover:underline flex items-center gap-2">
                        {t.changeAvatar}
                        <Icons.ChevronDown className={clsx("w-4 h-4 transition-transform", isSelectingAvatar && "rotate-180")} />
                    </button>
                    <div className="text-xs text-gray-400 mt-1">{t.avatarSize}</div>
                  </div>
                </div>

                <AnimatePresence>
                {isSelectingAvatar && (
                    <motion.div 
                        initial={{ height: 0, opacity: 0 }}
                        animate={{ height: 'auto', opacity: 1 }}
                        exit={{ height: 0, opacity: 0 }}
                        className="overflow-hidden"
                    >
                        <div className="grid grid-cols-4 sm:grid-cols-6 gap-3 p-4 bg-gray-50 rounded-2xl border border-gray-200 mb-6">
                            {CARTOON_AVATARS.map((avatarUrl, idx) => (
                                <button 
                                    key={idx}
                                    onClick={() => { updateUser({ avatar: avatarUrl }); }}
                                    className="relative aspect-square rounded-xl overflow-hidden bg-white shadow-sm hover:shadow-md transition-all group focus:outline-none"
                                >
                                    <img src={avatarUrl} alt={`Avatar ${idx}`} className="w-full h-full object-cover transition-transform group-hover:scale-110 duration-300" />
                                    
                                    {/* Selected State */}
                                    {user?.avatar === avatarUrl && (
                                        <motion.div 
                                            layoutId="avatar-selection-ring"
                                            className="absolute inset-0 border-4 border-black rounded-xl"
                                            initial={false}
                                            transition={{ type: "spring", stiffness: 300, damping: 30 }}
                                        />
                                    )}
                                    
                                    {/* Hover State */}
                                    <div className="absolute inset-0 bg-black/10 opacity-0 group-hover:opacity-100 transition-opacity" />
                                </button>
                            ))}
                        </div>
                    </motion.div>
                )}
                </AnimatePresence>

                <div className="grid gap-6">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">{t.displayName}</label>
                    <input 
                        type="text" 
                        value={tempName}
                        onChange={(e) => setTempName(e.target.value)}
                        className="w-full px-4 py-2.5 rounded-lg border border-gray-200 focus:border-black focus:ring-0 bg-gray-50 transition-all" 
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">{t.emailLabel}</label>
                    <input type="email" defaultValue={user?.email} disabled className="w-full px-4 py-2.5 rounded-lg border border-gray-200 bg-gray-100 text-gray-500 cursor-not-allowed" />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">{t.bio}</label>
                    <textarea 
                        rows={3} 
                        value={tempBio}
                        onChange={(e) => setTempBio(e.target.value)}
                        className="w-full px-4 py-2.5 rounded-lg border border-gray-200 focus:border-black focus:ring-0 bg-gray-50 transition-all" 
                        placeholder={t.bioPlaceholder}
                    />
                  </div>
                </div>

                <div className="flex justify-end pt-4">
                  <button onClick={handleSaveProfile} className="px-6 py-2.5 bg-black text-white rounded-lg font-medium hover:bg-gray-800 transition-colors shadow-lg hover:shadow-xl transform hover:-translate-y-0.5">
                    {t.saveChanges}
                  </button>
                </div>
              </div>
            </ModalContent>
          )}

          {activeModal === 'help' && (
             <ModalContent title={t.getHelp} icon={Icons.Help}>
               <div className="space-y-6">
                 <div className="bg-blue-50 p-4 rounded-xl border border-blue-100">
                   <h4 className="font-semibold text-blue-900 mb-1">{t.needHelpTitle}</h4>
                   <p className="text-sm text-blue-700">{t.needHelpDesc}</p>
                 </div>

                 <div className="space-y-4">
                    <details className="group border border-gray-200 rounded-lg overflow-hidden">
                      <summary className="flex items-center justify-between p-4 cursor-pointer bg-gray-50 hover:bg-gray-100 font-medium">
                        <span>{t.faq1Title}</span>
                        <Icons.ChevronDown className="w-4 h-4 text-gray-400 group-open:rotate-180 transition-transform" />
                      </summary>
                      <div className="p-4 text-sm text-gray-600 leading-relaxed border-t border-gray-200">
                        {t.faq1Desc}
                      </div>
                    </details>
                    <details className="group border border-gray-200 rounded-lg overflow-hidden">
                      <summary className="flex items-center justify-between p-4 cursor-pointer bg-gray-50 hover:bg-gray-100 font-medium">
                        <span>{t.faq2Title}</span>
                        <Icons.ChevronDown className="w-4 h-4 text-gray-400 group-open:rotate-180 transition-transform" />
                      </summary>
                      <div className="p-4 text-sm text-gray-600 leading-relaxed border-t border-gray-200">
                        {t.faq2Desc}
                      </div>
                    </details>
                    <details className="group border border-gray-200 rounded-lg overflow-hidden">
                      <summary className="flex items-center justify-between p-4 cursor-pointer bg-gray-50 hover:bg-gray-100 font-medium">
                        <span>{t.faq3Title}</span>
                        <Icons.ChevronDown className="w-4 h-4 text-gray-400 group-open:rotate-180 transition-transform" />
                      </summary>
                      <div className="p-4 text-sm text-gray-600 leading-relaxed border-t border-gray-200">
                        {t.faq3Desc}
                      </div>
                    </details>
                 </div>

                 <div className="pt-6 border-t border-gray-100">
                   <label className="block text-sm font-medium text-gray-700 mb-2">{t.contactSupport}</label>
                   <textarea rows={4} className="w-full px-4 py-3 rounded-lg border border-gray-200 focus:border-black focus:ring-0 bg-gray-50" placeholder={t.describeIssue}></textarea>
                   <button onClick={onClose} className="mt-3 w-full py-2.5 bg-white border border-gray-300 text-gray-700 font-medium rounded-lg hover:bg-gray-50 transition-colors">
                     {t.sendMessage}
                   </button>
                 </div>
               </div>
             </ModalContent>
          )}

          {activeModal === 'settings' && (
             <ModalContent title={t.settings} icon={Icons.Settings}>
               <div className="space-y-8">
                 
                 <section>
                   <h3 className="text-sm font-bold text-gray-400 uppercase tracking-wider mb-4">{t.general}</h3>
                   <div className="space-y-4">
                     <div className="flex items-center justify-between">
                       <div>
                         <div className="font-medium text-gray-900">{t.streamResponses}</div>
                         <div className="text-xs text-gray-500">{t.streamDesc}</div>
                       </div>
                       <div 
                         className={clsx("w-11 h-6 rounded-full relative cursor-pointer transition-colors", settings.streamResponses ? "bg-black" : "bg-gray-200")}
                         onClick={() => updateSettings({ streamResponses: !settings.streamResponses })}
                       >
                          <motion.div 
                            layout
                            className="absolute top-1 w-4 h-4 bg-white rounded-full shadow-sm" 
                            initial={false}
                            animate={{ left: settings.streamResponses ? '1.5rem' : '0.25rem' }}
                          />
                       </div>
                     </div>
                     <div className="flex items-center justify-between">
                       <div>
                         <div className="font-medium text-gray-900">{t.soundEffects}</div>
                         <div className="text-xs text-gray-500">{t.soundDesc}</div>
                       </div>
                       <div 
                         className={clsx("w-11 h-6 rounded-full relative cursor-pointer transition-colors", settings.soundEffects ? "bg-black" : "bg-gray-200")}
                         onClick={() => updateSettings({ soundEffects: !settings.soundEffects })}
                       >
                          <motion.div 
                            layout
                            className="absolute top-1 w-4 h-4 bg-white rounded-full shadow-sm" 
                            initial={false}
                            animate={{ left: settings.soundEffects ? '1.5rem' : '0.25rem' }}
                          />
                       </div>
                     </div>
                   </div>
                 </section>

                 <div className="h-px bg-gray-100" />

                 <section>
                   <h3 className="text-sm font-bold text-gray-400 uppercase tracking-wider mb-4">{t.dataPrivacy}</h3>
                   <div className="space-y-4">
                     <div className="flex items-center justify-between">
                       <div>
                         <div className="font-medium text-gray-900">{t.trainingData}</div>
                         <div className="text-xs text-gray-500">{t.trainingDesc}</div>
                       </div>
                       <div 
                         className={clsx("w-11 h-6 rounded-full relative cursor-pointer transition-colors", settings.allowTraining ? "bg-black" : "bg-gray-200")}
                         onClick={() => updateSettings({ allowTraining: !settings.allowTraining })}
                       >
                          <motion.div 
                            layout
                            className="absolute top-1 w-4 h-4 bg-white rounded-full shadow-sm" 
                            initial={false}
                            animate={{ left: settings.allowTraining ? '1.5rem' : '0.25rem' }}
                          />
                       </div>
                     </div>
                     <div className="flex items-center justify-between">
                        <button className="text-red-600 text-sm font-medium hover:underline">{t.clearData}</button>
                     </div>
                   </div>
                 </section>

                 <div className="h-px bg-gray-100" />

                 <div className="flex justify-end">
                    <button onClick={onClose} className="px-6 py-2.5 bg-black text-white rounded-lg font-medium hover:bg-gray-800 transition-colors">
                      {t.done}
                    </button>
                 </div>
               </div>
             </ModalContent>
          )}
        </ModalBackdrop>
      )}
    </AnimatePresence>
  );
};
