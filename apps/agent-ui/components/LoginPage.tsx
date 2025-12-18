
import React, { useState } from 'react';
import { useStore } from '../store';
import { Icons } from './icons';
import { translations } from '../locales';
import clsx from 'clsx';
import { motion } from 'framer-motion';

export const LoginPage: React.FC = () => {
  const login = useStore(s => s.login);
  const lang = useStore(s => s.language);
  const setLanguage = useStore(s => s.setLanguage);
  const t = translations[lang];

  const [isPhoneLogin, setIsPhoneLogin] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState({
    email: '',
    phone: '',
    password: ''
  });

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    try {
      await login({
        email: isPhoneLogin ? undefined : formData.email,
        phone: isPhoneLogin ? formData.phone : undefined,
        password: formData.password,
        provider: 'credentials'
      });
      // Store will update isAuthenticated, causing App to re-render main view
    } catch (error) {
      alert("Login Failed. Please try again.");
    } finally {
      setIsLoading(false);
    }
  };

  const handleSocialLogin = async (provider: 'google' | 'outlook') => {
    setIsLoading(true);
    try {
      await login({ provider });
    } catch (error) {
      alert("Login Failed");
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen w-full flex items-center justify-center bg-[#F3F4F6] text-gray-900 relative overflow-hidden">
      {/* Ambient Background */}
      <div className="absolute top-0 left-0 w-full h-full overflow-hidden z-0 pointer-events-none">
        <div className="absolute -top-40 -left-40 w-96 h-96 bg-blue-200 rounded-full mix-blend-multiply filter blur-3xl opacity-20 animate-pulse-slow"></div>
        <div className="absolute top-20 -right-20 w-72 h-72 bg-purple-200 rounded-full mix-blend-multiply filter blur-3xl opacity-20 animate-pulse-slow" style={{ animationDelay: '1s' }}></div>
      </div>

      <motion.div 
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        className="bg-white w-full max-w-md mx-4 p-8 rounded-3xl shadow-xl border border-gray-100 z-10"
      >
        {/* Header */}
        <div className="flex flex-col items-center mb-8">
          <div className="w-12 h-12 bg-black text-white rounded-xl flex items-center justify-center shadow-lg mb-4">
            <Icons.Zap className="w-7 h-7" fill="currentColor" />
          </div>
          <h1 className="text-2xl font-bold tracking-tight text-gray-900">{t.welcomeBack}</h1>
          <p className="text-sm text-gray-500 mt-1">{t.loginSubtitle}</p>
        </div>

        {/* Form */}
        <form onSubmit={handleLogin} className="space-y-4">
          
          {!isPhoneLogin ? (
            <div className="space-y-4">
               <div>
                 <label className="block text-xs font-semibold text-gray-700 uppercase mb-1.5 tracking-wider">{t.emailOrId}</label>
                 <div className="relative">
                   <Icons.User className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
                   <input 
                     type="text" 
                     required 
                     value={formData.email}
                     onChange={e => setFormData({...formData, email: e.target.value})}
                     className="w-full pl-10 pr-4 py-3 bg-gray-50 border border-gray-200 rounded-xl focus:ring-2 focus:ring-black focus:border-transparent outline-none transition-all"
                     placeholder="user@company.com" 
                   />
                 </div>
               </div>
               <div>
                 <div className="flex justify-between items-center mb-1.5">
                    <label className="block text-xs font-semibold text-gray-700 uppercase tracking-wider">{t.password}</label>
                    <a href="#" className="text-xs text-blue-600 hover:text-blue-800 hover:underline">{t.forgotPassword}</a>
                 </div>
                 <div className="relative">
                   <Icons.Settings className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" /> {/* Using settings icon as lock placeholder */}
                   <input 
                     type="password" 
                     required 
                     value={formData.password}
                     onChange={e => setFormData({...formData, password: e.target.value})}
                     className="w-full pl-10 pr-4 py-3 bg-gray-50 border border-gray-200 rounded-xl focus:ring-2 focus:ring-black focus:border-transparent outline-none transition-all"
                     placeholder="••••••••" 
                   />
                 </div>
               </div>
            </div>
          ) : (
            <div>
               <label className="block text-xs font-semibold text-gray-700 uppercase mb-1.5 tracking-wider">{t.phoneNumber}</label>
               <div className="relative">
                 <span className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500 font-medium border-r border-gray-300 pr-2 text-sm">+86</span>
                 <input 
                   type="tel" 
                   required 
                   value={formData.phone}
                   onChange={e => setFormData({...formData, phone: e.target.value})}
                   className="w-full pl-14 pr-4 py-3 bg-gray-50 border border-gray-200 rounded-xl focus:ring-2 focus:ring-black focus:border-transparent outline-none transition-all"
                   placeholder="138 0000 0000" 
                 />
               </div>
               <div className="mt-4">
                    <label className="block text-xs font-semibold text-gray-700 uppercase mb-1.5 tracking-wider">{t.password}</label>
                    <input 
                     type="password" 
                     required 
                     value={formData.password}
                     onChange={e => setFormData({...formData, password: e.target.value})}
                     className="w-full px-4 py-3 bg-gray-50 border border-gray-200 rounded-xl focus:ring-2 focus:ring-black focus:border-transparent outline-none transition-all"
                     placeholder="••••••••" 
                   />
               </div>
            </div>
          )}

          <button 
            type="button" 
            onClick={() => setIsPhoneLogin(!isPhoneLogin)}
            className="text-xs text-gray-500 hover:text-black transition-colors font-medium"
          >
            {isPhoneLogin ? t.useEmail : t.usePhone}
          </button>

          <button 
            type="submit"
            disabled={isLoading}
            className="w-full bg-black text-white py-3 rounded-xl font-bold text-sm hover:bg-gray-800 transition-all shadow-lg transform active:scale-[0.98] flex items-center justify-center"
          >
            {isLoading ? (
              <div className="w-5 h-5 border-2 border-white/30 border-t-white rounded-full animate-spin" />
            ) : (
              t.loginAction
            )}
          </button>
        </form>

        <div className="relative my-8">
            <div className="absolute inset-0 flex items-center"><div className="w-full border-t border-gray-200"></div></div>
            <div className="relative flex justify-center"><span className="bg-white px-4 text-xs text-gray-500 uppercase tracking-wider">{t.orContinueWith}</span></div>
        </div>

        <div className="grid grid-cols-2 gap-4">
            <button onClick={() => handleSocialLogin('google')} className="flex items-center justify-center gap-2 px-4 py-2.5 border border-gray-200 rounded-xl hover:bg-gray-50 transition-colors text-sm font-medium text-gray-700">
               <svg className="w-5 h-5" viewBox="0 0 24 24"><path d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z" fill="#4285F4"/><path d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z" fill="#34A853"/><path d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z" fill="#FBBC05"/><path d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z" fill="#EA4335"/></svg>
               Google
            </button>
            <button onClick={() => handleSocialLogin('outlook')} className="flex items-center justify-center gap-2 px-4 py-2.5 border border-gray-200 rounded-xl hover:bg-gray-50 transition-colors text-sm font-medium text-gray-700">
               <svg className="w-5 h-5" viewBox="0 0 24 24" fill="none"><path d="M1 5h22v14H1V5z" fill="#0078D4"/><path d="M1 5l11 8 11-8" stroke="#fff" strokeWidth="2"/></svg>
               Outlook
            </button>
        </div>

        {/* Language Switcher Small */}
        <div className="mt-8 flex justify-center">
            <div className="flex bg-gray-100 rounded-lg p-1">
                 <button 
                    onClick={() => setLanguage('en')}
                    className={clsx("px-3 py-1 text-xs font-medium rounded-md transition-all", lang === 'en' ? "bg-white text-black shadow-sm" : "text-gray-500")}
                 >
                    English
                 </button>
                 <button 
                    onClick={() => setLanguage('zh')}
                    className={clsx("px-3 py-1 text-xs font-medium rounded-md transition-all", lang === 'zh' ? "bg-white text-black shadow-sm" : "text-gray-500")}
                 >
                    中文
                 </button>
            </div>
        </div>
      </motion.div>

      <div className="absolute bottom-4 text-center w-full text-xs text-gray-400">
         © 2025 Agent Inc. All rights reserved.
      </div>
    </div>
  );
};
