
import React, { useState } from 'react';
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

const ModalContent: React.FC<{ children: React.ReactNode; title: string; icon?: any }> = ({ children, title, icon: Icon }) => (
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

// Optimized Cute Anime Style Avatars
const AVATAR_PARAMS = "&eyes=variant02,variant04,variant09,variant12,variant14&eyebrows=variant01,variant02,variant03,variant04&mouth=happy01,happy02,happy08,smile01";
const ANIME_AVATARS = [
  `https://api.dicebear.com/9.x/lorelei/svg?seed=Milo${AVATAR_PARAMS}`,
  `https://api.dicebear.com/9.x/lorelei/svg?seed=Sasha${AVATAR_PARAMS}`,
  `https://api.dicebear.com/9.x/lorelei/svg?seed=Felix${AVATAR_PARAMS}`,
  `https://api.dicebear.com/9.x/lorelei/svg?seed=Jazz${AVATAR_PARAMS}`,
  `https://api.dicebear.com/9.x/lorelei/svg?seed=Luna${AVATAR_PARAMS}`,
  `https://api.dicebear.com/9.x/lorelei/svg?seed=Leo${AVATAR_PARAMS}`,
  `https://api.dicebear.com/9.x/lorelei/svg?seed=Bella${AVATAR_PARAMS}`,
  `https://api.dicebear.com/9.x/lorelei/svg?seed=Zoe${AVATAR_PARAMS}`,
];

// Simple / Notion-like Avatars (Micah style from DiceBear)
const SIMPLE_AVATARS = [
    `https://api.dicebear.com/9.x/micah/svg?seed=Nala`,
    `https://api.dicebear.com/9.x/micah/svg?seed=Oliver`,
    `https://api.dicebear.com/9.x/micah/svg?seed=Bella`,
    `https://api.dicebear.com/9.x/micah/svg?seed=Leo`,
    `https://api.dicebear.com/9.x/micah/svg?seed=Max`,
    `https://api.dicebear.com/9.x/micah/svg?seed=Ruby`,
    `https://api.dicebear.com/9.x/micah/svg?seed=Charlie`,
    `https://api.dicebear.com/9.x/micah/svg?seed=Mia`,
];

export const UserModals: React.FC = () => {
  const activeModal = useStore(s => s.activeModal);
  const setActiveModal = useStore(s => s.setActiveModal);
  const language = useStore(s => s.language);
  const t = translations[language];
  const user = useStore(s => s.user);
  const updateUser = useStore(s => s.updateUser);
  const settings = useStore(s => s.settings);
  const updateSettings = useStore(s => s.updateSettings);

  const [displayName, setDisplayName] = useState(user?.name || '');
  const [bio, setBio] = useState(user?.bio || '');

  const closeModal = () => setActiveModal(null);

  const handleSaveAccount = () => {
    updateUser({ name: displayName, bio });
    closeModal();
  };

  const clearData = () => {
      if (confirm('Are you sure you want to clear all data? This will reset the application state.')) {
          localStorage.clear();
          window.location.reload();
      }
  };

  if (!activeModal) return null;

  return (
    <AnimatePresence>
      <ModalBackdrop onClose={closeModal}>
        {activeModal === 'upgrade' && (
          <ModalContent title={t.upgradeSubscription} icon={Icons.CreditCard}>
             {/* Upgrade Content */}
             <div className="text-center mb-8">
                <div className="w-16 h-16 bg-gradient-to-tr from-yellow-200 to-yellow-500 rounded-2xl flex items-center justify-center mx-auto mb-4 shadow-lg shadow-yellow-200">
                    <Icons.Zap className="w-8 h-8 text-white" fill="currentColor" />
                </div>
                <h3 className="text-2xl font-bold text-gray-900">{t.unleashPotential}</h3>
                <p className="text-gray-500 mt-2 max-w-md mx-auto">{t.getAccessPro}</p>
             </div>

             <div className="grid md:grid-cols-2 gap-4">
                {/* Free Plan */}
                <div className="border border-gray-200 rounded-xl p-5 relative opacity-70 hover:opacity-100 transition-opacity">
                    <h4 className="font-bold text-lg mb-2">{t.freePlan}</h4>
                    <div className="text-3xl font-bold mb-4">$0 <span className="text-sm font-normal text-gray-400">{t.perMonth}</span></div>
                    <ul className="space-y-3 text-sm text-gray-600 mb-6">
                        <li className="flex items-center gap-2"><Icons.Check className="w-4 h-4 text-green-500" /> {t.standardSpeed}</li>
                        <li className="flex items-center gap-2"><Icons.Check className="w-4 h-4 text-green-500" /> {t.dailyLimits}</li>
                        <li className="flex items-center gap-2"><Icons.Check className="w-4 h-4 text-green-500" /> {t.accessKimi}</li>
                    </ul>
                    <button className="w-full py-2 bg-gray-100 text-gray-700 font-bold rounded-lg text-sm cursor-default">Current Plan</button>
                </div>

                {/* Pro Plan */}
                <div className="border-2 border-black rounded-xl p-5 relative shadow-xl transform scale-[1.02] bg-white">
                    <div className="absolute top-0 right-0 bg-black text-white text-[10px] font-bold px-2 py-1 rounded-bl-lg uppercase tracking-wider">{t.recommended}</div>
                    <h4 className="font-bold text-lg mb-2">{t.proPlanCard}</h4>
                    <div className="text-3xl font-bold mb-4">$20 <span className="text-sm font-normal text-gray-400">{t.perMonth}</span></div>
                    <ul className="space-y-3 text-sm text-gray-600 mb-6">
                        <li className="flex items-center gap-2"><Icons.Zap className="w-4 h-4 text-yellow-500" fill="currentColor" /> {t.fastSpeed}</li>
                        <li className="flex items-center gap-2"><Icons.Check className="w-4 h-4 text-green-500" /> {t.unlimitedChats}</li>
                        <li className="flex items-center gap-2"><Icons.Brain className="w-4 h-4 text-purple-500" /> {t.accessDeepseek}</li>
                        <li className="flex items-center gap-2"><Icons.User className="w-4 h-4 text-blue-500" /> {t.prioritySupport}</li>
                    </ul>
                    <button className="w-full py-2 bg-black text-white hover:bg-gray-800 font-bold rounded-lg text-sm transition-all shadow-md hover:shadow-lg">{t.upgradeBtn}</button>
                </div>
             </div>
             <p className="text-center text-xs text-gray-400 mt-6 flex items-center justify-center gap-1">
                <Icons.CreditCard className="w-3 h-3" />
                {t.securePayment}
             </p>
          </ModalContent>
        )}

        {activeModal === 'account' && (
          <ModalContent title={t.account} icon={Icons.User}>
             <div className="space-y-6">
                {/* Avatar Section */}
                <div>
                    <label className="block text-sm font-bold text-gray-700 mb-3">{t.changeAvatar}</label>
                    <div className="space-y-4">
                        <div>
                             <span className="text-xs font-semibold text-gray-400 uppercase tracking-wider mb-2 block">{t.styleAnime}</span>
                             <div className="flex gap-4 overflow-x-auto pb-2 scrollbar-hide">
                                {ANIME_AVATARS.map((url, idx) => (
                                    <button 
                                        key={idx}
                                        onClick={() => updateUser({ avatar: url })}
                                        className={clsx(
                                            "relative w-16 h-16 rounded-full border-2 transition-all flex-shrink-0 overflow-hidden",
                                            user?.avatar === url ? "border-black ring-2 ring-black/20" : "border-transparent hover:border-gray-300"
                                        )}
                                    >
                                        <img src={url} alt={`Anime ${idx}`} className="w-full h-full object-cover" />
                                        {user?.avatar === url && (
                                            <div className="absolute inset-0 bg-black/20 flex items-center justify-center">
                                                <Icons.Check className="w-6 h-6 text-white drop-shadow-md" />
                                            </div>
                                        )}
                                    </button>
                                ))}
                            </div>
                        </div>

                        <div>
                             <span className="text-xs font-semibold text-gray-400 uppercase tracking-wider mb-2 block">{t.styleSimple}</span>
                             <div className="flex gap-4 overflow-x-auto pb-2 scrollbar-hide">
                                {SIMPLE_AVATARS.map((url, idx) => (
                                    <button 
                                        key={idx}
                                        onClick={() => updateUser({ avatar: url })}
                                        className={clsx(
                                            "relative w-16 h-16 rounded-full border-2 transition-all flex-shrink-0 overflow-hidden",
                                            user?.avatar === url ? "border-black ring-2 ring-black/20" : "border-transparent hover:border-gray-300"
                                        )}
                                    >
                                        <img src={url} alt={`Simple ${idx}`} className="w-full h-full object-cover" />
                                        {user?.avatar === url && (
                                            <div className="absolute inset-0 bg-black/20 flex items-center justify-center">
                                                <Icons.Check className="w-6 h-6 text-white drop-shadow-md" />
                                            </div>
                                        )}
                                    </button>
                                ))}
                            </div>
                        </div>
                    </div>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div>
                        <label className="block text-sm font-bold text-gray-700 mb-1.5">{t.displayName}</label>
                        <input 
                            type="text" 
                            value={displayName} 
                            onChange={(e) => setDisplayName(e.target.value)}
                            className="w-full px-4 py-2 bg-gray-50 border border-gray-200 rounded-lg focus:ring-2 focus:ring-black focus:border-transparent outline-none"
                        />
                    </div>
                    <div>
                        <label className="block text-sm font-bold text-gray-700 mb-1.5">{t.emailLabel}</label>
                        <input 
                            type="email" 
                            value={user?.email} 
                            disabled
                            className="w-full px-4 py-2 bg-gray-100 border border-gray-200 rounded-lg text-gray-500 cursor-not-allowed"
                        />
                    </div>
                </div>

                <div>
                    <label className="block text-sm font-bold text-gray-700 mb-1.5">{t.bio}</label>
                    <textarea 
                        value={bio}
                        onChange={(e) => setBio(e.target.value)}
                        placeholder={t.bioPlaceholder}
                        rows={3}
                        className="w-full px-4 py-2 bg-gray-50 border border-gray-200 rounded-lg focus:ring-2 focus:ring-black focus:border-transparent outline-none resize-none"
                    />
                </div>

                <div className="pt-4 border-t border-gray-100 flex justify-end">
                    <button 
                        onClick={handleSaveAccount}
                        className="px-6 py-2 bg-black text-white font-bold rounded-lg hover:bg-gray-800 transition-colors"
                    >
                        {t.saveChanges}
                    </button>
                </div>
             </div>
          </ModalContent>
        )}

        {activeModal === 'settings' && (
            <ModalContent title={t.settings} icon={Icons.Settings}>
                <div className="space-y-6">
                    <div>
                        <h3 className="text-sm font-bold text-gray-900 mb-3 uppercase tracking-wider">{t.general}</h3>
                        <div className="space-y-3">
                            <label className="flex items-center justify-between p-3 bg-gray-50 rounded-xl hover:bg-gray-100 transition-colors cursor-pointer">
                                <div>
                                    <div className="font-medium text-gray-900">{t.streamResponses}</div>
                                    <div className="text-xs text-gray-500">{t.streamDesc}</div>
                                </div>
                                <div className={clsx("w-11 h-6 bg-gray-200 rounded-full relative transition-colors", settings.streamResponses && "bg-green-500")}>
                                    <input 
                                        type="checkbox" 
                                        className="hidden" 
                                        checked={settings.streamResponses} 
                                        onChange={(e) => updateSettings({ streamResponses: e.target.checked })}
                                    />
                                    <div className={clsx("w-5 h-5 bg-white rounded-full shadow-sm absolute top-0.5 transition-all", settings.streamResponses ? "left-[22px]" : "left-0.5")} />
                                </div>
                            </label>
                            
                            <label className="flex items-center justify-between p-3 bg-gray-50 rounded-xl hover:bg-gray-100 transition-colors cursor-pointer">
                                <div>
                                    <div className="font-medium text-gray-900">{t.soundEffects}</div>
                                    <div className="text-xs text-gray-500">{t.soundDesc}</div>
                                </div>
                                <div className={clsx("w-11 h-6 bg-gray-200 rounded-full relative transition-colors", settings.soundEffects && "bg-green-500")}>
                                    <input 
                                        type="checkbox" 
                                        className="hidden" 
                                        checked={settings.soundEffects} 
                                        onChange={(e) => updateSettings({ soundEffects: e.target.checked })}
                                    />
                                    <div className={clsx("w-5 h-5 bg-white rounded-full shadow-sm absolute top-0.5 transition-all", settings.soundEffects ? "left-[22px]" : "left-0.5")} />
                                </div>
                            </label>
                        </div>
                    </div>

                    <div>
                        <h3 className="text-sm font-bold text-gray-900 mb-3 uppercase tracking-wider">{t.dataPrivacy}</h3>
                        <div className="space-y-3">
                            <label className="flex items-center justify-between p-3 bg-gray-50 rounded-xl hover:bg-gray-100 transition-colors cursor-pointer">
                                <div>
                                    <div className="font-medium text-gray-900">{t.trainingData}</div>
                                    <div className="text-xs text-gray-500">{t.trainingDesc}</div>
                                </div>
                                <div className={clsx("w-11 h-6 bg-gray-200 rounded-full relative transition-colors", settings.allowTraining && "bg-green-500")}>
                                    <input 
                                        type="checkbox" 
                                        className="hidden" 
                                        checked={settings.allowTraining} 
                                        onChange={(e) => updateSettings({ allowTraining: e.target.checked })}
                                    />
                                    <div className={clsx("w-5 h-5 bg-white rounded-full shadow-sm absolute top-0.5 transition-all", settings.allowTraining ? "left-[22px]" : "left-0.5")} />
                                </div>
                            </label>
                            
                            <button 
                                onClick={clearData}
                                className="w-full flex items-center justify-between p-3 bg-red-50 text-red-600 rounded-xl hover:bg-red-100 transition-colors"
                            >
                                <span className="font-medium">{t.clearData}</span>
                                <Icons.Trash className="w-4 h-4" />
                            </button>
                        </div>
                    </div>
                </div>
            </ModalContent>
        )}

        {activeModal === 'help' && (
            <ModalContent title={t.getHelp} icon={Icons.Help}>
                <div className="space-y-6">
                    <div className="bg-blue-50 p-4 rounded-xl flex items-start gap-3">
                        <div className="p-2 bg-blue-100 text-blue-600 rounded-lg">
                            <Icons.Chat className="w-5 h-5" />
                        </div>
                        <div>
                            <h4 className="font-bold text-blue-900">{t.needHelpTitle}</h4>
                            <p className="text-sm text-blue-700 mt-1">{t.needHelpDesc}</p>
                        </div>
                    </div>

                    <div className="space-y-4">
                        <h3 className="font-bold text-gray-900">FAQ</h3>
                        <div className="space-y-2">
                             <div className="border border-gray-200 rounded-lg p-3">
                                <h5 className="font-medium text-gray-900 text-sm">{t.faq1Title}</h5>
                                <p className="text-xs text-gray-500 mt-1">{t.faq1Desc}</p>
                             </div>
                             <div className="border border-gray-200 rounded-lg p-3">
                                <h5 className="font-medium text-gray-900 text-sm">{t.faq2Title}</h5>
                                <p className="text-xs text-gray-500 mt-1">{t.faq2Desc}</p>
                             </div>
                             <div className="border border-gray-200 rounded-lg p-3">
                                <h5 className="font-medium text-gray-900 text-sm">{t.faq3Title}</h5>
                                <p className="text-xs text-gray-500 mt-1">{t.faq3Desc}</p>
                             </div>
                        </div>
                    </div>

                    <div className="pt-4 border-t border-gray-100">
                        <h3 className="font-bold text-gray-900 mb-3">{t.contactSupport}</h3>
                        <div className="flex gap-2">
                            <input type="text" placeholder={t.describeIssue} className="flex-1 px-4 py-2 bg-gray-50 border border-gray-200 rounded-lg text-sm focus:ring-2 focus:ring-black outline-none" />
                            <button className="px-4 py-2 bg-black text-white font-bold rounded-lg text-sm hover:bg-gray-800 transition-colors">
                                {t.sendMessage}
                            </button>
                        </div>
                    </div>
                </div>
            </ModalContent>
        )}
      </ModalBackdrop>
    </AnimatePresence>
  );
};
