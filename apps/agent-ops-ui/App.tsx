
import React, { useState, useEffect } from 'react';
import { 
  LayoutDashboard, Server, Database, Folder, 
  Settings as SettingsIcon, Terminal as TerminalIcon, Search, Bell, Globe, Layers, Zap, Moon, Sun, HardDrive, User, LogOut, ChevronsLeft, ChevronsRight, Command
} from 'lucide-react';
import Dashboard from './components/Dashboard';
import MLOps from './components/MLOps';
import Kubernetes from './components/Kubernetes';
import Terminal from './components/Terminal';
import LocalModel from './components/LocalModel';
import DataTools from './components/DataTools';
import Projects from './components/Projects';
import Filesystem from './components/Filesystem';
import Settings from './components/Settings';
import Login from './components/Login';
import { ViewState, Language, ThemeMode } from './types';
import { TRANSLATIONS } from './constants';

const App = () => {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [view, setView] = useState<ViewState>(ViewState.DASHBOARD);
  const [sidebarOpen, setSidebarOpen] = useState(true);
  const [terminalOpen, setTerminalOpen] = useState(true);
  // Default to English as requested
  const [lang, setLang] = useState<Language>('en'); 
  const [theme, setTheme] = useState<ThemeMode>('light'); 
  const [userMenuOpen, setUserMenuOpen] = useState(false);
  const [activeSettingsTab, setActiveSettingsTab] = useState('system');

  useEffect(() => {
    if (theme === 'dark') {
      document.documentElement.classList.add('dark');
    } else {
      document.documentElement.classList.remove('dark');
    }
  }, [theme]);

  const t = TRANSLATIONS[lang];

  const handleLogin = () => {
    setIsAuthenticated(true);
    setView(ViewState.DASHBOARD);
  };

  const handleLogout = () => {
    setIsAuthenticated(false);
    setUserMenuOpen(false);
  };

  const handleNavigateSettings = (tab: string) => {
    setActiveSettingsTab(tab);
    setView(ViewState.SETTINGS);
    setUserMenuOpen(false);
  };

  const NavItem = ({ viewTarget, icon: Icon, label, onClick }: { viewTarget: ViewState, icon: any, label: string, onClick?: () => void }) => (
    <button 
      onClick={() => {
        if (onClick) onClick();
        setView(viewTarget);
      }}
      className={`w-full flex items-center group relative transition-all duration-300 px-4 py-2.5 rounded-xl ${
        view === viewTarget 
        ? 'bg-nexus-accent text-white shadow-lg shadow-nexus-accent/20' 
        : 'text-slate-500 dark:text-nexus-400 hover:bg-slate-100 dark:hover:bg-nexus-800/50 hover:text-nexus-900 dark:hover:text-white'
      } ${!sidebarOpen ? 'justify-center' : 'space-x-3.5'}`}
    >
      <Icon size={18} className={`${view === viewTarget ? 'text-white' : 'group-hover:scale-110 transition-transform'}`} />
      {sidebarOpen && <span className="font-bold text-[11px] uppercase tracking-wider truncate">{label}</span>}
      {!sidebarOpen && (
        <div className="absolute left-16 invisible group-hover:visible bg-nexus-900 text-white text-[10px] font-black uppercase tracking-widest py-2 px-3 rounded-lg shadow-2xl z-50 pointer-events-none whitespace-nowrap border border-nexus-700">
           {label}
        </div>
      )}
    </button>
  );

  const NavCategory = ({ label }: { label: string }) => (
    <div className="mt-8 mb-2 px-4">
      <div className="flex items-center">
        {sidebarOpen ? (
          <>
            <span className="text-[9px] font-black uppercase tracking-[0.2em] whitespace-nowrap text-slate-400 dark:text-nexus-500 mr-2">
              {label}
            </span>
            <div className="h-[1px] flex-1 bg-slate-100 dark:bg-nexus-800"></div>
          </>
        ) : (
          <div className="h-[1px] w-full bg-slate-100 dark:bg-nexus-800"></div>
        )}
      </div>
    </div>
  );

  if (!isAuthenticated) {
    return <Login onLogin={handleLogin} lang={lang} />;
  }

  return (
    <div className="flex h-screen bg-light-bg dark:bg-nexus-900 text-light-text dark:text-nexus-100 overflow-hidden font-sans selection:bg-nexus-accent selection:text-white transition-colors duration-500">
      
      {/* Optimized Sidebar */}
      <aside 
        className={`${sidebarOpen ? 'w-64' : 'w-20'} bg-light-surface dark:bg-nexus-900 border-r border-slate-200 dark:border-nexus-800/50 flex flex-col transition-all duration-500 z-20 shadow-xl`}
      >
        {/* Sidebar Header with Toggle in Top Right */}
        <div className="h-20 flex items-center px-4 relative group/header border-b border-transparent">
          <div className={`flex items-center transition-all ${!sidebarOpen && 'mx-auto'}`}>
            <div className="w-10 h-10 bg-nexus-accent rounded-2xl flex items-center justify-center shrink-0 shadow-xl shadow-nexus-accent/20 rotate-3 group-hover:rotate-0 transition-transform cursor-pointer">
              <Globe size={22} className="text-white" />
            </div>
            {sidebarOpen && (
              <div className="ml-3">
                <span className="text-lg font-black tracking-tighter text-nexus-900 dark:text-white leading-none block">Manus<span className="text-nexus-accent font-light italic">Project</span></span>
                <span className="text-[8px] font-black uppercase tracking-[0.3em] text-nexus-500 block mt-1">Intelligence Core</span>
              </div>
            )}
          </div>
          
          {/* Top Right Indent Toggle Button */}
          <button 
             onClick={() => setSidebarOpen(!sidebarOpen)}
             className={`absolute ${sidebarOpen ? 'top-6 right-3' : 'top-6 right-2'} p-1.5 rounded-lg transition-all duration-300 border border-transparent hover:bg-slate-100 dark:hover:bg-nexus-800 text-slate-400 dark:text-nexus-500 z-30`}
             title={sidebarOpen ? "Collapse Menu" : "Expand Menu"}
           >
             {sidebarOpen ? (
                <ChevronsLeft size={16} />
             ) : (
               <ChevronsRight size={14} />
             )}
          </button>
        </div>

        <nav className="flex-1 overflow-y-auto px-4 custom-scrollbar pb-10">
          <div className="pt-4">
            <NavItem viewTarget={ViewState.DASHBOARD} icon={LayoutDashboard} label={t.dashboard} />
          </div>

          <NavCategory label={t.catMachineLearning} />
          <div className="space-y-1">
            <NavItem viewTarget={ViewState.MLOPS} icon={Layers} label={t.mlops} />
            <NavItem viewTarget={ViewState.LOCAL_MODEL} icon={Zap} label={t.localModel} />
          </div>

          <NavCategory label={t.catProjectInfo} />
          <div className="space-y-1">
            <NavItem viewTarget={ViewState.KUBERNETES} icon={Server} label={t.kubernetes} />
            <NavItem viewTarget={ViewState.DATA_TOOLS} icon={Database} label={t.dataTools} />
            <NavItem viewTarget={ViewState.PROJECTS} icon={Folder} label={t.projects} />
          </div>

          <NavCategory label={t.catStorage} />
          <div className="space-y-1">
            <NavItem viewTarget={ViewState.FILESYSTEM} icon={HardDrive} label={t.filesystem} />
          </div>
          
          <NavCategory label={t.catSystem} />
          <div className="space-y-1">
            <NavItem 
              viewTarget={ViewState.SETTINGS} 
              icon={SettingsIcon} 
              label={t.settings} 
              onClick={() => setActiveSettingsTab('system')}
            />
          </div>
        </nav>
        
        {/* Minimal Sidebar Bottom Dock (Terminal Only) */}
        <div className="p-4 border-t border-slate-100 dark:border-nexus-800/50">
           <button 
             onClick={() => setTerminalOpen(!terminalOpen)}
             className={`w-full flex items-center justify-center p-3 rounded-2xl bg-slate-50 dark:bg-nexus-900/50 border border-slate-200 dark:border-nexus-800/50 hover:border-nexus-accent text-slate-500 dark:text-nexus-400 hover:text-nexus-accent transition-all group`}
           >
              <TerminalIcon size={18} />
           </button>
        </div>
      </aside>

      {/* Main Layout Area */}
      <main className="flex-1 flex flex-col relative overflow-hidden">
        
        <header className="h-20 bg-light-surface/70 dark:bg-nexus-900/70 backdrop-blur-xl border-b border-slate-200 dark:border-nexus-800/50 flex items-center justify-between px-10 z-10">
          <div className="flex items-center space-x-6">
             <div className="hidden sm:flex items-center space-x-3 text-[10px] font-black uppercase tracking-widest text-slate-400 dark:text-nexus-500">
                <Command size={14} />
                <span>ROOT</span>
                <span>/</span>
                <span className="text-nexus-accent font-bold tracking-widest">{view}</span>
             </div>
             
             <div className="relative hidden md:block w-80">
                <Search className="absolute left-4 top-1/2 transform -translate-y-1/2 text-nexus-500" size={14} />
                <input 
                  type="text" 
                  placeholder={t.searchPlaceholder}
                  className="w-full bg-slate-100 dark:bg-nexus-800/50 border border-transparent focus:border-nexus-accent/30 rounded-2xl py-2 pl-12 pr-4 text-xs text-nexus-900 dark:text-white placeholder-nexus-500 transition-all outline-none"
                />
             </div>
          </div>

          <div className="flex items-center space-x-6">
             <div className="flex items-center space-x-1 bg-slate-100 dark:bg-nexus-800/50 p-1 rounded-xl shadow-inner border border-slate-200 dark:border-nexus-700">
                <button 
                  onClick={() => setLang(prev => prev === 'en' ? 'zh' : 'en')}
                  className="px-3 py-1.5 rounded-lg text-[10px] font-black transition-all hover:bg-white dark:hover:bg-nexus-700 text-nexus-500 dark:text-nexus-300"
                >
                  {lang === 'en' ? 'EN' : 'ZH'}
                </button>
                <div className="w-[1px] h-3 bg-slate-200 dark:bg-nexus-700"></div>
                <button 
                  onClick={() => setTheme(prev => prev === 'light' ? 'dark' : 'light')}
                  className="px-3 py-1.5 rounded-lg transition-all hover:bg-white dark:hover:bg-nexus-700 text-nexus-500 dark:text-nexus-300"
                >
                  {theme === 'light' ? <Sun size={14} /> : <Moon size={14} />}
                </button>
             </div>

             <button className="relative p-3 rounded-2xl bg-slate-100 dark:bg-nexus-800/50 border border-slate-200 dark:border-nexus-700 text-slate-500 dark:text-nexus-400 hover:text-nexus-accent transition-all shadow-sm">
                <Bell size={18} />
                <span className="absolute top-2.5 right-2.5 w-2 h-2 bg-nexus-accent rounded-full border-2 border-white dark:border-nexus-900 shadow-[0_0_10px_rgba(59,130,246,0.5)]"></span>
             </button>

             <div className="relative">
                <button 
                   onClick={() => setUserMenuOpen(!userMenuOpen)}
                   className="flex items-center space-x-3 p-1 pr-3 rounded-2xl bg-slate-100 dark:bg-nexus-800/50 border border-slate-200 dark:border-nexus-700 hover:border-nexus-accent/30 transition-all shadow-sm"
                >
                   <div className="w-8 h-8 rounded-xl bg-gradient-to-tr from-blue-500 to-purple-500 shadow-lg border border-white/20"></div>
                   <div className="text-left hidden lg:block text-[10px] uppercase font-black">
                     <p className="text-slate-800 dark:text-white tracking-widest leading-none mb-0.5">Admin</p>
                     <p className="text-nexus-500 tracking-tighter leading-none">Verified</p>
                   </div>
                </button>
                
                {userMenuOpen && (
                   <div className="absolute right-0 mt-4 w-60 bg-white dark:bg-nexus-800 border border-slate-200 dark:border-nexus-700 rounded-3xl shadow-2xl z-50 animate-fade-in overflow-hidden p-2">
                      <div className="px-4 py-4 mb-2 bg-slate-50 dark:bg-nexus-900/50 rounded-2xl text-[10px] uppercase font-black">
                         <p className="text-slate-900 dark:text-white tracking-widest">Administrator</p>
                         <p className="text-slate-400 dark:text-nexus-500 font-bold truncate lowercase">sys_ops_001@opsnexus.io</p>
                      </div>
                      <div className="space-y-1">
                         <button onClick={() => handleNavigateSettings('profile')} className="flex w-full items-center px-4 py-3 text-[10px] font-black uppercase tracking-widest text-slate-600 dark:text-nexus-300 hover:bg-slate-50 dark:hover:bg-nexus-700 rounded-xl transition-colors">
                            <User size={14} className="mr-3 text-nexus-accent" /> {t.profile}
                         </button>
                         <button onClick={() => handleNavigateSettings('account')} className="flex w-full items-center px-4 py-3 text-[10px] font-black uppercase tracking-widest text-slate-600 dark:text-nexus-300 hover:bg-slate-50 dark:hover:bg-nexus-700 rounded-xl transition-colors">
                            <SettingsIcon size={14} className="mr-3 text-nexus-accent" /> {t.accountSettings}
                         </button>
                      </div>
                      <div className="mt-2 pt-2 border-t border-slate-100 dark:border-nexus-700">
                         <button onClick={handleLogout} className="flex w-full items-center px-4 py-3 text-[10px] font-black uppercase tracking-widest text-red-500 hover:bg-red-50 dark:hover:bg-red-900/20 rounded-xl transition-colors">
                            <LogOut size={14} className="mr-3" /> {t.logout}
                         </button>
                      </div>
                   </div>
                )}
             </div>
          </div>
        </header>

        <div className="flex-1 overflow-y-auto p-10 bg-light-bg dark:bg-nexus-900 relative custom-scrollbar">
          <div className="max-w-[1600px] mx-auto">
            {view === ViewState.DASHBOARD && <Dashboard lang={lang} />}
            {view === ViewState.MLOPS && <MLOps lang={lang} />}
            {view === ViewState.LOCAL_MODEL && <LocalModel lang={lang} />}
            {view === ViewState.KUBERNETES && <Kubernetes lang={lang} />}
            {view === ViewState.DATA_TOOLS && <DataTools lang={lang} />}
            {view === ViewState.PROJECTS && <Projects lang={lang} />}
            {view === ViewState.FILESYSTEM && <Filesystem lang={lang} />}
            {view === ViewState.SETTINGS && <Settings lang={lang} activeTab={activeSettingsTab} />}
          </div>
        </div>

        <Terminal isOpen={terminalOpen} onToggle={() => setTerminalOpen(!terminalOpen)} />
      </main>
    </div>
  );
};

export default App;
