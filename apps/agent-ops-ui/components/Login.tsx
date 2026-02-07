
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
      await authService.login(username, password);
      onLogin(); 
    } catch (err: any) {
      setError(err.message || 'Login failed');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="flex h-screen w-full bg-slate-50 dark:bg-nexus-900 text-slate-900 dark:text-white font-sans overflow-hidden transition-colors duration-500">
      
      {/* Left Side - Visual (70%) - Bright Theme */}
      <div className="w-[70%] relative hidden lg:flex flex-col justify-center items-center overflow-hidden bg-slate-100">
         {/* Abstract Bright Background */}
         <div className="absolute inset-0">
             <div className="absolute top-0 left-0 w-full h-full bg-[url('https://images.unsplash.com/photo-1618005182384-a83a8bd57fbe?q=80&w=2564&auto=format&fit=crop')] bg-cover bg-center opacity-80"></div>
             <div className="absolute inset-0 bg-gradient-to-r from-white/60 via-white/40 to-transparent backdrop-blur-[2px]"></div>
             
             {/* Decorative Elements */}
             <div className="absolute top-1/4 left-1/4 w-96 h-96 bg-blue-400/20 rounded-full blur-3xl animate-pulse mix-blend-multiply"></div>
             <div className="absolute bottom-1/4 right-1/4 w-64 h-64 bg-purple-400/20 rounded-full blur-3xl animate-pulse mix-blend-multiply" style={{animationDelay: '1s'}}></div>
         </div>

         <div className="relative z-10 text-center space-y-6 max-w-2xl px-8">
            <div className="inline-flex items-center justify-center p-4 bg-white/40 backdrop-blur-xl rounded-2xl border border-white/50 shadow-xl mb-8">
               <Globe size={64} className="text-nexus-accent drop-shadow-sm" />
            </div>
            <h1 className="text-5xl font-extrabold tracking-tight text-slate-900">
               AgentProject <span className="text-nexus-accent font-light">Control Plane</span>
            </h1>
            <p className="text-lg text-slate-600 font-medium leading-relaxed max-w-lg mx-auto">
               Next-generation unified management for MLOps pipelines, Kubernetes clusters, and distributed data systems. 
            </p>
            <div className="flex justify-center space-x-8 pt-8 text-sm text-slate-500 uppercase tracking-widest font-mono font-bold">
               <span className="flex items-center"><ShieldCheck size={16} className="mr-2 text-emerald-600" /> Enterprise Grade</span>
               <span className="flex items-center"><Lock size={16} className="mr-2 text-blue-600" /> End-to-End Encrypted</span>
            </div>
         </div>
      </div>

      {/* Right Side - Form (30%) - Bright Theme */}
      <div className="w-full lg:w-[30%] bg-white dark:bg-nexus-800 flex flex-col justify-center px-12 relative border-l border-slate-200 dark:border-nexus-700 shadow-2xl">
         <div className="max-w-md w-full mx-auto space-y-8">
            <div className="text-center lg:text-left">
               <div className="lg:hidden flex justify-center mb-6">
                  <div className="w-12 h-12 bg-nexus-accent rounded-lg flex items-center justify-center">
                    <Globe size={24} className="text-white" />
                  </div>
               </div>
               <h2 className="text-2xl font-bold text-slate-900 dark:text-white mb-2 tracking-tight">{t.loginTitle}</h2>
               <p className="text-slate-500 dark:text-nexus-400 text-sm font-medium">{t.loginSubtitle}</p>
            </div>

            {error && (
               <div className="bg-red-50 border border-red-200 text-red-600 dark:bg-red-500/10 dark:border-red-500/50 dark:text-red-400 rounded-xl p-4 flex items-center text-sm font-medium">
                  <AlertCircle size={18} className="mr-3" />
                  {error}
               </div>
            )}

            <form className="space-y-6" onSubmit={handleSubmit}>
               <div className="space-y-5">
                  <div>
                     <label className="block text-xs font-bold text-slate-500 dark:text-nexus-300 uppercase mb-2 ml-1 tracking-wider">{t.username}</label>
                     <div className="relative">
                        <User className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" size={18} />
                        <input 
                          type="text" 
                          className="w-full bg-slate-50 dark:bg-nexus-900 border border-slate-200 dark:border-nexus-600 rounded-xl py-3.5 pl-11 pr-4 text-slate-900 dark:text-white placeholder-slate-400 focus:outline-none focus:border-nexus-accent focus:ring-2 focus:ring-nexus-accent/20 transition-all font-medium"
                          placeholder="admin@agentproject.io"
                          value={username}
                          onChange={(e) => setUsername(e.target.value)}
                        />
                     </div>
                  </div>
                  <div>
                     <label className="block text-xs font-bold text-slate-500 dark:text-nexus-300 uppercase mb-2 ml-1 tracking-wider">{t.password}</label>
                     <div className="relative">
                        <Lock className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" size={18} />
                        <input 
                          type="password" 
                          className="w-full bg-slate-50 dark:bg-nexus-900 border border-slate-200 dark:border-nexus-600 rounded-xl py-3.5 pl-11 pr-4 text-slate-900 dark:text-white placeholder-slate-400 focus:outline-none focus:border-nexus-accent focus:ring-2 focus:ring-nexus-accent/20 transition-all font-medium"
                          placeholder="••••••••"
                          value={password}
                          onChange={(e) => setPassword(e.target.value)}
                        />
                     </div>
                  </div>
               </div>

               <div className="flex items-center justify-between text-xs font-medium">
                  <div className="flex items-center">
                     <input type="checkbox" className="h-4 w-4 rounded bg-slate-100 dark:bg-nexus-900 border-slate-300 dark:border-nexus-600 text-nexus-accent focus:ring-offset-0 cursor-pointer" />
                     <label className="ml-2 text-slate-600 dark:text-nexus-300">Remember me</label>
                  </div>
                  <a href="#" className="text-nexus-accent hover:text-blue-600 dark:hover:text-blue-400">Forgot password?</a>
               </div>

               <button 
                 type="submit"
                 disabled={isLoading}
                 className="w-full bg-nexus-accent hover:bg-blue-600 text-white font-bold py-3.5 rounded-xl shadow-lg shadow-blue-500/20 transition-all transform hover:scale-[1.02] flex justify-center items-center disabled:opacity-50 disabled:cursor-not-allowed uppercase tracking-wider text-xs"
               >
                 {isLoading ? <Loader size={20} className="animate-spin" /> : t.login}
               </button>
            </form>

            <div className="text-center text-xs font-medium text-slate-400 dark:text-nexus-500 mt-8">
               &copy; {new Date().getFullYear()} AgentProject Inc. All rights reserved.
            </div>
         </div>
      </div>
    </div>
  );
};

export default Login;
