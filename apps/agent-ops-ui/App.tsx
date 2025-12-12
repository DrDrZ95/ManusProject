import React, { useState, useEffect } from 'react';
import { 
  LayoutDashboard, Server, Database, Folder, 
  Settings as SettingsIcon, Terminal as TerminalIcon, Search, Bell, Menu, X, Globe, Layers, Zap, Moon, Sun, HardDrive, User, LogOut
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
  const [lang, setLang] = useState<Language>('zh'); // Default Chinese
  const [theme, setTheme] = useState<ThemeMode>('light'); // Default Light
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
      className={`w-full flex items-center space-x-3 px-4 py-3 rounded-lg transition-all duration-200 group ${
        view === viewTarget 
        ? 'bg-nexus-accent/10 text-nexus-accent border-r-2 border-nexus-accent' 
        : 'text-light-textSec dark:text-nexus-400 hover:bg-nexus-100 dark:hover:bg-nexus-800 hover:text-nexus-900 dark:hover:text-white'
      }`}
    >
      <Icon size={20} className={view === viewTarget ? 'text-nexus-accent' : 'group-hover:text-nexus-900 dark:group-hover:text-white transition-colors'} />
      <span className="font-medium">{label}</span>
    </button>
  );

  // If not authenticated, show Login Screen
  if (!isAuthenticated) {
    return <Login onLogin={handleLogin} lang={lang} />;
  }

  return (
    <div className="flex h-screen bg-light-bg dark:bg-nexus-900 text-light-text dark:text-nexus-100 overflow-hidden font-sans selection:bg-nexus-accent selection:text-white">
      
      {/* Sidebar */}
      <aside 
        className={`${sidebarOpen ? 'w-64' : 'w-20'} bg-light-surface dark:bg-nexus-900 border-r border-light-border dark:border-nexus-700 flex flex-col transition-all duration-300 z-20`}
      >
        <div className="h-16 flex items-center justify-center border-b border-light-border dark:border-nexus-700">
          {sidebarOpen ? (
            <div className="text-2xl font-bold tracking-tighter text-nexus-900 dark:text-white flex items-center">
              <div className="w-8 h-8 bg-nexus-accent rounded-lg mr-2 flex items-center justify-center">
                <Globe size={18} className="text-white" />
              </div>
              OpsNexus
            </div>
          ) : (
             <div className="w-10 h-10 bg-nexus-accent rounded-lg flex items-center justify-center">
                <Globe size={24} className="text-white" />
             </div>
          )}
        </div>

        <nav className="flex-1 overflow-y-auto py-6 px-3 space-y-2">
          <NavItem viewTarget={ViewState.DASHBOARD} icon={LayoutDashboard} label={sidebarOpen ? t.dashboard : ""} />
          <NavItem viewTarget={ViewState.MLOPS} icon={Layers} label={sidebarOpen ? t.mlops : ""} />
          <NavItem viewTarget={ViewState.LOCAL_MODEL} icon={Zap} label={sidebarOpen ? t.localModel : ""} />
          <NavItem viewTarget={ViewState.KUBERNETES} icon={Server} label={sidebarOpen ? t.kubernetes : ""} />
          <NavItem viewTarget={ViewState.DATA_TOOLS} icon={Database} label={sidebarOpen ? t.dataTools : ""} />
          <NavItem viewTarget={ViewState.PROJECTS} icon={Folder} label={sidebarOpen ? t.projects : ""} />
          <NavItem viewTarget={ViewState.FILESYSTEM} icon={HardDrive} label={sidebarOpen ? t.filesystem : ""} />
          <div className="pt-6 mt-6 border-t border-light-border dark:border-nexus-800">
             <NavItem 
               viewTarget={ViewState.SETTINGS} 
               icon={SettingsIcon} 
               label={sidebarOpen ? t.settings : ""} 
               onClick={() => setActiveSettingsTab('system')}
             />
          </div>
        </nav>
        
        <div className="p-4 border-t border-light-border dark:border-nexus-700">
           <button 
             onClick={() => setTerminalOpen(!terminalOpen)}
             className={`w-full flex items-center justify-center p-2 rounded-lg bg-nexus-100 dark:bg-nexus-800 hover:bg-nexus-200 dark:hover:bg-nexus-700 text-nexus-600 dark:text-nexus-300 transition-colors border border-light-border dark:border-nexus-700 ${!sidebarOpen && 'px-0'}`}
           >
              <TerminalIcon size={18} className={sidebarOpen ? "mr-2" : ""} />
              {sidebarOpen && <span className="text-sm font-mono">CLI</span>}
           </button>
        </div>
      </aside>

      {/* Main Content */}
      <main className="flex-1 flex flex-col relative overflow-hidden">
        
        {/* Top Header */}
        <header className="h-16 bg-light-surface/80 dark:bg-nexus-900/80 backdrop-blur border-b border-light-border dark:border-nexus-700 flex items-center justify-between px-6 z-10">
          <div className="flex items-center">
             <button onClick={() => setSidebarOpen(!sidebarOpen)} className="mr-4 text-light-textSec dark:text-nexus-400 hover:text-nexus-900 dark:hover:text-white">
               {sidebarOpen ? <X size={20} /> : <Menu size={20} />}
             </button>
             <div className="relative hidden md:block w-96">
                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-nexus-400" size={16} />
                <input 
                  type="text" 
                  placeholder={t.searchPlaceholder}
                  className="w-full bg-nexus-100 dark:bg-nexus-800 border border-light-border dark:border-nexus-700 rounded-full py-1.5 pl-10 pr-4 text-sm text-nexus-900 dark:text-white placeholder-nexus-400 dark:placeholder-nexus-500 focus:outline-none focus:border-nexus-accent focus:ring-1 focus:ring-nexus-accent transition-all"
                />
             </div>
          </div>
          <div className="flex items-center space-x-4">
             {/* Lang Toggle */}
             <button 
                onClick={() => setLang(prev => prev === 'en' ? 'zh' : 'en')}
                className="p-2 rounded-full hover:bg-nexus-100 dark:hover:bg-nexus-800 text-sm font-bold text-nexus-600 dark:text-nexus-300"
             >
                {lang === 'en' ? 'EN' : '中'}
             </button>
             
             {/* Theme Toggle */}
             <button 
                onClick={() => setTheme(prev => prev === 'light' ? 'dark' : 'light')}
                className="p-2 rounded-full hover:bg-nexus-100 dark:hover:bg-nexus-800 text-nexus-600 dark:text-nexus-300"
             >
                {theme === 'light' ? <Sun size={20} /> : <Moon size={20} />}
             </button>

             <button className="relative text-light-textSec dark:text-nexus-400 hover:text-nexus-900 dark:hover:text-white transition-colors">
                <Bell size={20} />
                <span className="absolute top-0 right-0 w-2 h-2 bg-red-500 rounded-full"></span>
             </button>

             {/* User Profile Dropdown */}
             <div className="relative">
                <button 
                   onClick={() => setUserMenuOpen(!userMenuOpen)}
                   className="flex items-center space-x-2 focus:outline-none"
                >
                   <div className="w-8 h-8 rounded-full bg-gradient-to-tr from-blue-500 to-purple-500 border border-light-border dark:border-nexus-600 cursor-pointer"></div>
                </button>
                
                {userMenuOpen && (
                   <div className="absolute right-0 mt-2 w-48 bg-light-surface dark:bg-nexus-800 border border-light-border dark:border-nexus-700 rounded-lg shadow-xl z-50 animate-fade-in overflow-hidden">
                      <div className="px-4 py-3 border-b border-light-border dark:border-nexus-700">
                         <p className="text-sm font-medium text-light-text dark:text-white">Admin User</p>
                         <p className="text-xs text-light-textSec dark:text-nexus-400 truncate">admin@opsnexus.io</p>
                      </div>
                      <div className="py-1">
                         <button 
                            onClick={() => handleNavigateSettings('profile')}
                            className="flex w-full items-center px-4 py-2 text-sm text-light-text dark:text-nexus-200 hover:bg-nexus-100 dark:hover:bg-nexus-700"
                         >
                            <User size={14} className="mr-2" /> {t.profile}
                         </button>
                         <button 
                            onClick={() => handleNavigateSettings('account')}
                            className="flex w-full items-center px-4 py-2 text-sm text-light-text dark:text-nexus-200 hover:bg-nexus-100 dark:hover:bg-nexus-700"
                         >
                            <SettingsIcon size={14} className="mr-2" /> {t.accountSettings}
                         </button>
                      </div>
                      <div className="py-1 border-t border-light-border dark:border-nexus-700">
                         <button 
                            onClick={handleLogout}
                            className="flex w-full items-center px-4 py-2 text-sm text-red-500 hover:bg-red-50 dark:hover:bg-red-900/20"
                         >
                            <LogOut size={14} className="mr-2" /> {t.logout}
                         </button>
                      </div>
                   </div>
                )}
             </div>
          </div>
        </header>

        {/* View Area */}
        <div className="flex-1 overflow-y-auto p-6 bg-light-bg/50 dark:bg-nexus-900/50 relative">
          {view === ViewState.DASHBOARD && <Dashboard lang={lang} />}
          {view === ViewState.MLOPS && <MLOps lang={lang} />}
          {view === ViewState.LOCAL_MODEL && <LocalModel lang={lang} />}
          {view === ViewState.KUBERNETES && <Kubernetes lang={lang} />}
          {view === ViewState.DATA_TOOLS && <DataTools lang={lang} />}
          {view === ViewState.PROJECTS && <Projects lang={lang} />}
          {view === ViewState.FILESYSTEM && <Filesystem lang={lang} />}
          {view === ViewState.SETTINGS && <Settings lang={lang} activeTab={activeSettingsTab} />}
        </div>

        {/* Terminal Overlay */}
        <Terminal isOpen={terminalOpen} onToggle={() => setTerminalOpen(!terminalOpen)} />

      </main>
    </div>
  );
};

export default App;