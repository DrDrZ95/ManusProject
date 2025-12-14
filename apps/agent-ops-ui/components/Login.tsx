import React, { useState } from 'react';
import { Globe, ShieldCheck, Lock, User, Loader, AlertCircle } from 'lucide-react';
import { TRANSLATIONS } from '../constants';
import { Language } from '../types';
import { authService } from '../services/auth';

interface LoginProps {
  onLogin: () => void;
  lang: Language;
}

const Login: React.FC<LoginProps> = ({ onLogin, lang }) => {
  const t = TRANSLATIONS[lang];
  const [username, setUsername] = useState('admin');
  const [password, setPassword] = useState('password');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setError(null);

    try {
      // 使用独立的 Auth Service 进行登录
      await authService.login(username, password);
      onLogin(); // 通知 App 组件更新状态
    } catch (err: any) {
      setError(err.message || 'Login failed');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="flex h-screen w-full bg-nexus-900 text-white font-sans overflow-hidden">
      
      {/* Left Side - Visual (70%) */}
      <div className="w-[70%] relative hidden lg:flex flex-col justify-center items-center overflow-hidden">
         {/* Abstract Background */}
         <div className="absolute inset-0 bg-nexus-900">
             <div className="absolute top-0 left-0 w-full h-full bg-[url('https://images.unsplash.com/photo-1550751827-4bd374c3f58b?q=80&w=2070&auto=format&fit=crop')] bg-cover bg-center opacity-20 mix-blend-overlay"></div>
             <div className="absolute inset-0 bg-gradient-to-r from-nexus-900 via-transparent to-transparent"></div>
             
             {/* Decorative Elements */}
             <div className="absolute top-1/4 left-1/4 w-96 h-96 bg-blue-500/20 rounded-full blur-3xl animate-pulse"></div>
             <div className="absolute bottom-1/4 right-1/4 w-64 h-64 bg-purple-500/20 rounded-full blur-3xl animate-pulse" style={{animationDelay: '1s'}}></div>
         </div>

         <div className="relative z-10 text-center space-y-6 max-w-2xl px-8">
            <div className="inline-flex items-center justify-center p-4 bg-white/5 backdrop-blur-lg rounded-2xl border border-white/10 shadow-2xl mb-8">
               <Globe size={64} className="text-nexus-accent" />
            </div>
            <h1 className="text-5xl font-bold tracking-tight bg-clip-text text-transparent bg-gradient-to-r from-blue-400 to-purple-400">
               OpsNexus Control Plane
            </h1>
            <p className="text-lg text-nexus-300 font-light leading-relaxed">
               Next-generation unified management for MLOps pipelines, Kubernetes clusters, and distributed data systems. 
               Secure, observable, and automated.
            </p>
            <div className="flex justify-center space-x-8 pt-8 text-sm text-nexus-400 uppercase tracking-widest font-mono">
               <span className="flex items-center"><ShieldCheck size={16} className="mr-2 text-green-400" /> Enterprise Grade</span>
               <span className="flex items-center"><Lock size={16} className="mr-2 text-blue-400" /> End-to-End Encrypted</span>
            </div>
         </div>
      </div>

      {/* Right Side - Form (30%) */}
      <div className="w-full lg:w-[30%] bg-nexus-800 flex flex-col justify-center px-12 relative border-l border-nexus-700 shadow-2xl">
         <div className="max-w-md w-full mx-auto space-y-8">
            <div className="text-center lg:text-left">
               <div className="lg:hidden flex justify-center mb-6">
                  <div className="w-12 h-12 bg-nexus-accent rounded-lg flex items-center justify-center">
                    <Globe size={24} className="text-white" />
                  </div>
               </div>
               <h2 className="text-2xl font-bold text-white mb-2">{t.loginTitle}</h2>
               <p className="text-nexus-400 text-sm">{t.loginSubtitle}</p>
            </div>

            {error && (
               <div className="bg-red-500/10 border border-red-500/50 rounded-lg p-3 flex items-center text-red-400 text-sm">
                  <AlertCircle size={16} className="mr-2" />
                  {error}
               </div>
            )}

            <form className="space-y-6" onSubmit={handleSubmit}>
               <div className="space-y-4">
                  <div>
                     <label className="block text-xs font-medium text-nexus-300 uppercase mb-2 ml-1">{t.username}</label>
                     <div className="relative">
                        <User className="absolute left-3 top-1/2 -translate-y-1/2 text-nexus-500" size={18} />
                        <input 
                          type="text" 
                          className="w-full bg-nexus-900 border border-nexus-600 rounded-lg py-3 pl-10 pr-4 text-white placeholder-nexus-600 focus:outline-none focus:border-nexus-accent focus:ring-1 focus:ring-nexus-accent transition-all"
                          placeholder="admin@opsnexus.io"
                          value={username}
                          onChange={(e) => setUsername(e.target.value)}
                        />
                     </div>
                  </div>
                  <div>
                     <label className="block text-xs font-medium text-nexus-300 uppercase mb-2 ml-1">{t.password}</label>
                     <div className="relative">
                        <Lock className="absolute left-3 top-1/2 -translate-y-1/2 text-nexus-500" size={18} />
                        <input 
                          type="password" 
                          className="w-full bg-nexus-900 border border-nexus-600 rounded-lg py-3 pl-10 pr-4 text-white placeholder-nexus-600 focus:outline-none focus:border-nexus-accent focus:ring-1 focus:ring-nexus-accent transition-all"
                          placeholder="••••••••"
                          value={password}
                          onChange={(e) => setPassword(e.target.value)}
                        />
                     </div>
                  </div>
               </div>

               <div className="flex items-center justify-between text-xs">
                  <div className="flex items-center">
                     <input type="checkbox" className="h-4 w-4 rounded bg-nexus-900 border-nexus-600 text-nexus-accent focus:ring-offset-nexus-800" />
                     <label className="ml-2 text-nexus-300">Remember me</label>
                  </div>
                  <a href="#" className="text-nexus-accent hover:text-blue-400">Forgot password?</a>
               </div>

               <button 
                 type="submit"
                 disabled={isLoading}
                 className="w-full bg-nexus-accent hover:bg-blue-600 text-white font-bold py-3 rounded-lg shadow-lg shadow-blue-500/20 transition-all transform hover:scale-[1.02] flex justify-center items-center disabled:opacity-50 disabled:cursor-not-allowed"
               >
                 {isLoading ? <Loader size={20} className="animate-spin" /> : t.login}
               </button>
            </form>

            <div className="text-center text-xs text-nexus-500 mt-8">
               &copy; 2024 OpsNexus Inc. All rights reserved.
            </div>
         </div>
      </div>
    </div>
  );
};

export default Login;