
import React from 'react';
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

export const UserModals: React.FC = () => {
  const activeModal = useStore(s => s.activeModal);
  const setActiveModal = useStore(s => s.setActiveModal);
  const lang = useStore(s => s.language);
  const t = translations[lang];

  const onClose = () => setActiveModal(null);

  if (!activeModal) return null;

  return (
    <AnimatePresence>
      {activeModal && (
        <ModalBackdrop onClose={onClose}>
          {activeModal === 'upgrade' && (
            <ModalContent title={t.upgradeSubscription} icon={Icons.Zap}>
              <div className="space-y-6">
                <div className="text-center mb-8">
                  <h3 className="text-2xl font-bold mb-2">Unleash Full Potential</h3>
                  <p className="text-gray-500">Get access to Agent Pro with advanced reasoning and faster speeds.</p>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div className="border border-gray-200 rounded-xl p-6 hover:border-blue-500 transition-colors cursor-pointer relative">
                    <div className="text-lg font-semibold mb-2">Agent Free</div>
                    <div className="text-3xl font-bold mb-4">$0<span className="text-sm text-gray-400 font-normal">/mo</span></div>
                    <ul className="space-y-3 text-sm text-gray-600">
                      <li className="flex items-center gap-2"><Icons.Check className="w-4 h-4 text-green-500" /> Standard Response Speed</li>
                      <li className="flex items-center gap-2"><Icons.Check className="w-4 h-4 text-green-500" /> Daily Conversation Limits</li>
                      <li className="flex items-center gap-2"><Icons.Check className="w-4 h-4 text-green-500" /> Access to Kimi Basic</li>
                    </ul>
                  </div>

                  <div className="border-2 border-black rounded-xl p-6 relative shadow-lg bg-gray-50">
                    <div className="absolute top-0 right-0 bg-black text-white text-xs font-bold px-3 py-1 rounded-bl-xl rounded-tr-lg">RECOMMENDED</div>
                    <div className="text-lg font-semibold mb-2">Agent Pro</div>
                    <div className="text-3xl font-bold mb-4">$20<span className="text-sm text-gray-400 font-normal">/mo</span></div>
                    <ul className="space-y-3 text-sm text-gray-600">
                      <li className="flex items-center gap-2"><Icons.Check className="w-4 h-4 text-black" /> Fast Response Speed</li>
                      <li className="flex items-center gap-2"><Icons.Check className="w-4 h-4 text-black" /> Unlimited Conversations</li>
                      <li className="flex items-center gap-2"><Icons.Check className="w-4 h-4 text-black" /> Access to Deepseek & GPT-OSS</li>
                      <li className="flex items-center gap-2"><Icons.Check className="w-4 h-4 text-black" /> Priority Support</li>
                    </ul>
                  </div>
                </div>

                <div className="mt-8 pt-6 border-t border-gray-100">
                  <button className="w-full bg-black text-white py-4 rounded-xl font-bold text-lg hover:bg-gray-800 transition-all shadow-lg transform hover:scale-[1.01]">
                    Upgrade to Pro
                  </button>
                  <p className="text-center text-xs text-gray-400 mt-4">Secure payment processed by Stripe. Cancel anytime.</p>
                </div>
              </div>
            </ModalContent>
          )}

          {activeModal === 'account' && (
            <ModalContent title={t.account} icon={Icons.User}>
              <div className="space-y-6">
                <div className="flex items-center gap-6 pb-6 border-b border-gray-100">
                  <div className="w-20 h-20 rounded-full bg-gradient-to-br from-gray-200 to-gray-300 flex items-center justify-center shadow-inner">
                    <Icons.User className="w-8 h-8 text-gray-500" />
                  </div>
                  <div>
                    <button className="text-sm font-medium text-blue-600 hover:underline">Change Avatar</button>
                    <div className="text-xs text-gray-400 mt-1">JPG, GIF or PNG. Max size of 800K</div>
                  </div>
                </div>

                <div className="grid gap-6">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">Display Name</label>
                    <input type="text" defaultValue="Agent User" className="w-full px-4 py-2.5 rounded-lg border border-gray-200 focus:border-black focus:ring-0 bg-gray-50" />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">Email Address</label>
                    <input type="email" defaultValue="user@example.com" disabled className="w-full px-4 py-2.5 rounded-lg border border-gray-200 bg-gray-100 text-gray-500 cursor-not-allowed" />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">Bio</label>
                    <textarea rows={3} className="w-full px-4 py-2.5 rounded-lg border border-gray-200 focus:border-black focus:ring-0 bg-gray-50" placeholder="Tell us about yourself..."></textarea>
                  </div>
                </div>

                <div className="flex justify-end pt-4">
                  <button onClick={onClose} className="px-6 py-2.5 bg-black text-white rounded-lg font-medium hover:bg-gray-800 transition-colors">
                    Save Changes
                  </button>
                </div>
              </div>
            </ModalContent>
          )}

          {activeModal === 'help' && (
             <ModalContent title={t.getHelp} icon={Icons.Help}>
               <div className="space-y-6">
                 <div className="bg-blue-50 p-4 rounded-xl border border-blue-100">
                   <h4 className="font-semibold text-blue-900 mb-1">Need immediate assistance?</h4>
                   <p className="text-sm text-blue-700">Our support team is available 24/7 to help you with any issues.</p>
                 </div>

                 <div className="space-y-4">
                    <details className="group border border-gray-200 rounded-lg overflow-hidden">
                      <summary className="flex items-center justify-between p-4 cursor-pointer bg-gray-50 hover:bg-gray-100 font-medium">
                        <span>How do I use the Linux Terminal?</span>
                        <Icons.ChevronDown className="w-4 h-4 text-gray-400 group-open:rotate-180 transition-transform" />
                      </summary>
                      <div className="p-4 text-sm text-gray-600 leading-relaxed border-t border-gray-200">
                        The terminal is a simulated environment connected to the backend. You can run standard Linux commands like `ls`, `cd`, and `cat`. Currently, it operates in a restricted sandbox mode.
                      </div>
                    </details>
                    <details className="group border border-gray-200 rounded-lg overflow-hidden">
                      <summary className="flex items-center justify-between p-4 cursor-pointer bg-gray-50 hover:bg-gray-100 font-medium">
                        <span>Is my data private?</span>
                        <Icons.ChevronDown className="w-4 h-4 text-gray-400 group-open:rotate-180 transition-transform" />
                      </summary>
                      <div className="p-4 text-sm text-gray-600 leading-relaxed border-t border-gray-200">
                        Yes, all conversation data is stored locally in your browser (Local Storage) and is not sent to any server other than the AI inference provider for generation.
                      </div>
                    </details>
                    <details className="group border border-gray-200 rounded-lg overflow-hidden">
                      <summary className="flex items-center justify-between p-4 cursor-pointer bg-gray-50 hover:bg-gray-100 font-medium">
                        <span>Can I export my chats?</span>
                        <Icons.ChevronDown className="w-4 h-4 text-gray-400 group-open:rotate-180 transition-transform" />
                      </summary>
                      <div className="p-4 text-sm text-gray-600 leading-relaxed border-t border-gray-200">
                        Currently, you can email chats to yourself using the context menu on the sidebar. Full export features are coming soon.
                      </div>
                    </details>
                 </div>

                 <div className="pt-6 border-t border-gray-100">
                   <label className="block text-sm font-medium text-gray-700 mb-2">Contact Support</label>
                   <textarea rows={4} className="w-full px-4 py-3 rounded-lg border border-gray-200 focus:border-black focus:ring-0 bg-gray-50" placeholder="Describe your issue..."></textarea>
                   <button onClick={onClose} className="mt-3 w-full py-2.5 bg-white border border-gray-300 text-gray-700 font-medium rounded-lg hover:bg-gray-50 transition-colors">
                     Send Message
                   </button>
                 </div>
               </div>
             </ModalContent>
          )}

          {activeModal === 'settings' && (
             <ModalContent title={t.settings} icon={Icons.Settings}>
               <div className="space-y-8">
                 
                 <section>
                   <h3 className="text-sm font-bold text-gray-400 uppercase tracking-wider mb-4">General</h3>
                   <div className="space-y-4">
                     <div className="flex items-center justify-between">
                       <div>
                         <div className="font-medium text-gray-900">Stream Responses</div>
                         <div className="text-xs text-gray-500">Show text as it is being generated</div>
                       </div>
                       <div className="w-11 h-6 bg-black rounded-full relative cursor-pointer">
                          <div className="absolute right-1 top-1 w-4 h-4 bg-white rounded-full shadow-sm" />
                       </div>
                     </div>
                     <div className="flex items-center justify-between">
                       <div>
                         <div className="font-medium text-gray-900">Sound Effects</div>
                         <div className="text-xs text-gray-500">Play subtle sounds for messages</div>
                       </div>
                       <div className="w-11 h-6 bg-gray-200 rounded-full relative cursor-pointer">
                          <div className="absolute left-1 top-1 w-4 h-4 bg-white rounded-full shadow-sm" />
                       </div>
                     </div>
                   </div>
                 </section>

                 <div className="h-px bg-gray-100" />

                 <section>
                   <h3 className="text-sm font-bold text-gray-400 uppercase tracking-wider mb-4">Data & Privacy</h3>
                   <div className="space-y-4">
                     <div className="flex items-center justify-between">
                       <div>
                         <div className="font-medium text-gray-900">Training Data</div>
                         <div className="text-xs text-gray-500">Allow conversations to be used for training</div>
                       </div>
                       <div className="w-11 h-6 bg-gray-200 rounded-full relative cursor-pointer">
                          <div className="absolute left-1 top-1 w-4 h-4 bg-white rounded-full shadow-sm" />
                       </div>
                     </div>
                     <div className="flex items-center justify-between">
                        <button className="text-red-600 text-sm font-medium hover:underline">Clear All Data</button>
                     </div>
                   </div>
                 </section>

                 <div className="h-px bg-gray-100" />

                 <div className="flex justify-end">
                    <button onClick={onClose} className="px-6 py-2.5 bg-black text-white rounded-lg font-medium hover:bg-gray-800 transition-colors">
                      Done
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
