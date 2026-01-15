
import React, { useState, useRef, useEffect } from 'react';
import { useStore } from '../store';
import { Icons } from './icons';
import clsx from 'clsx';
import { AnimatePresence, motion } from 'framer-motion';
import { translations } from '../locales';
import { ChatSession, Group } from '../types';
import { v4 as uuidv4 } from 'uuid';

const ContextMenu = ({ x, y, onClose, actions }: any) => {
  const menuRef = useRef<HTMLDivElement>(null);
  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) onClose();
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, [onClose]);

  return (
    <div 
      ref={menuRef}
      className="fixed z-[100] w-56 bg-white/95 backdrop-blur-xl rounded-2xl shadow-2xl border border-gray-200/50 py-2 text-sm animate-fadeIn"
      style={{ top: Math.min(y, window.innerHeight - 300), left: Math.min(x, window.innerWidth - 240) }}
    >
      {actions.filter(Boolean).map((action: any, idx: number) => {
        if (action.separator) {
          return <div key={idx} className="h-px bg-gray-100 my-1.5 mx-2" />;
        }

        if (action.subMenu) {
          return (
            <div key={idx} className="relative group/submenu">
              <button className="w-full px-4 py-2.5 text-left flex items-center gap-3 hover:bg-gray-50 text-gray-700 font-bold transition-colors">
                {action.icon && <action.icon className="w-4 h-4 text-gray-400" />}
                <span className="flex-1">{action.label}</span>
                <Icons.ChevronRight className="w-3.5 h-3.5 text-gray-400" />
              </button>
              <div className="absolute left-full top-0 -ml-1 w-48 bg-white rounded-2xl shadow-2xl border border-gray-200 py-1.5 hidden group-hover/submenu:block">
                {action.subMenu.filter(Boolean).map((sub: any, sidx: number) => (
                  <button 
                    key={sidx} 
                    onClick={() => { sub.onClick(); onClose(); }} 
                    className="w-full px-4 py-2 text-left hover:bg-gray-50 text-[11px] font-black truncate text-gray-500 hover:text-black transition-colors"
                  >
                    {sub.label || "Untitled"}
                  </button>
                ))}
              </div>
            </div>
          );
        }

        if (!action.label) return null;

        return (
          <button 
            key={idx} 
            onClick={() => { action.onClick?.(); onClose(); }} 
            className={clsx(
              "w-full px-4 py-2.5 text-left flex items-center gap-3 font-bold transition-colors", 
              action.danger ? "text-red-600 hover:bg-red-50" : "text-gray-700 hover:bg-gray-50"
            )}
          >
            {action.icon && <action.icon className={clsx("w-4 h-4", action.danger ? "text-red-500" : "text-gray-400")} />}
            <span>{action.label}</span>
          </button>
        );
      })}
    </div>
  );
};

const ProjectMarker = ({ group, isSlim }: { group: Group, isSlim?: boolean }) => (
  <div className={clsx(
    "flex-shrink-0 rounded-lg flex items-center justify-center font-black text-white shadow-sm ring-1 ring-black/5 transition-transform group-hover/item:scale-105",
    isSlim ? "w-8 h-8 text-xs" : "w-6 h-6 text-[10px]",
    group.color
  )}>
    {group.marker.charAt(0)}
  </div>
);

const NavItem = ({ isActive, isEditing, editValue, onEditChange, onEditSubmit, onClick, onContextMenu, onActionClick, children, icon: Icon, group, isSlim, isSubItem }: any) => (
  <div className={clsx("relative group/item mb-0.5", isSlim ? "px-2" : isSubItem ? "pr-2" : "px-4")}>
    <button
      onClick={onClick}
      onContextMenu={onContextMenu}
      title={isSlim ? children : undefined}
      className={clsx(
        "w-full h-10 flex items-center rounded-xl transition-all duration-200 relative",
        isSlim ? "justify-center px-0" : "px-2 gap-3",
        isActive ? "bg-gray-200/50 text-black font-black" : "text-gray-600 hover:bg-gray-200/30",
        isSubItem && !isSlim && "h-9"
      )}
    >
      {group ? (
        <ProjectMarker group={group} isSlim={isSlim} />
      ) : Icon ? (
        <Icon className={clsx(isSlim ? "w-5 h-5" : "w-4 h-4", "shrink-0", isActive ? "text-black" : "text-gray-400 group-hover:text-black")} />
      ) : (
        isSlim ? (
          <div className={clsx("w-8 h-8 rounded-lg flex items-center justify-center transition-colors", isActive ? "bg-black text-white" : "bg-gray-100 text-gray-400")}>
            <Icons.Chat className="w-4 h-4" />
          </div>
        ) : null
      )}
      
      {!isSlim && (
        <div className="flex-1 min-w-0 pr-4">
          {isEditing ? (
            <input 
              autoFocus 
              className="w-full bg-transparent outline-none text-sm font-black border-b-2 border-black/10 focus:border-black py-0.5"
              value={editValue}
              onChange={(e) => onEditChange(e.target.value)}
              onBlur={() => onEditSubmit()}
              onKeyDown={(e) => {
                if (e.key === 'Enter') onEditSubmit();
                if (e.key === 'Escape') onEditSubmit(true);
              }}
              onClick={(e) => e.stopPropagation()}
            />
          ) : (
            <span className={clsx("block truncate text-[13px] tracking-tight transition-opacity text-left", isActive ? "opacity-100" : "opacity-80 font-bold")}>
                {children || "Untitled"}
            </span>
          )}
        </div>
      )}

      {isActive && !isEditing && (
        <div className={clsx(
          "absolute top-1/2 -translate-y-1/2 w-1 h-4 bg-black rounded-r-full shadow-sm shadow-black/20",
          isSlim ? "left-0" : isSubItem ? "-left-[1px]" : "left-0"
        )} />
      )}
    </button>
    
    {!isEditing && !isSlim && (
      <button 
        onClick={(e) => {
          e.stopPropagation();
          onActionClick?.(e);
        }}
        className="absolute right-4 top-1/2 -translate-y-1/2 p-1 text-gray-400 hover:text-black opacity-0 group-hover/item:opacity-100 transition-opacity rounded-md hover:bg-white"
      >
        <Icons.MoreVertical className="w-3.5 h-3.5" />
      </button>
    )}
  </div>
);

export const Sidebar: React.FC = () => {
  const store = useStore();
  const t = translations[store.language];
  const isSlim = !store.isSidebarOpen;
  const sidebarWidth = store.sidebarWidth;
  
  const [contextMenu, setContextMenu] = useState<any>(null);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [editValue, setEditValue] = useState('');
  const [showUserMenu, setShowUserMenu] = useState(false);
  const [isResizing, setIsResizing] = useState(false);
  
  const [isProjectsCollapsed, setIsProjectsCollapsed] = useState(false);
  const [isRecentCollapsed, setIsRecentCollapsed] = useState(false);
  
  const userMenuRef = useRef<HTMLDivElement>(null);

  const activeGroups = store.groups.filter(g => !g.isDeleted);
  const activeSessions = store.sessions.filter(s => !s.isDeleted);

  const groupedSessions = activeGroups.map(g => ({ group: g, sessions: activeSessions.filter(s => s.groupId === g.id) }));
  const ungrouped = activeSessions.filter(s => !s.groupId);

  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (userMenuRef.current && !userMenuRef.current.contains(e.target as Node)) setShowUserMenu(false);
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  useEffect(() => {
    const handleMouseMove = (e: MouseEvent) => {
      if (isResizing) {
        const newWidth = Math.max(220, Math.min(500, e.clientX));
        store.setSidebarWidth(newWidth);
      }
    };
    const handleMouseUp = () => setIsResizing(false);

    if (isResizing) {
      document.addEventListener('mousemove', handleMouseMove);
      document.addEventListener('mouseup', handleMouseUp);
      document.body.style.cursor = 'col-resize';
      document.body.style.userSelect = 'none';
    } else {
      document.body.style.cursor = 'default';
      document.body.style.userSelect = 'auto';
    }

    return () => {
      document.removeEventListener('mousemove', handleMouseMove);
      document.removeEventListener('mouseup', handleMouseUp);
    };
  }, [isResizing]);

  const handleRenameSubmit = (cancel = false) => {
    if (!editingId || cancel) {
      setEditingId(null);
      return;
    }
    if (editValue.trim()) {
      const group = store.groups.find(g => g.id === editingId);
      if (group) store.updateGroup(editingId, { title: editValue });
      else store.renameSession(editingId, editValue);
    }
    setEditingId(null);
  };

  const startRename = (id: string, initialValue: string) => {
    setEditingId(id);
    setEditValue(initialValue);
  };

  const onContextMenuGroup = (e: any, g: Group) => {
    e.preventDefault();
    setContextMenu({ x: e.clientX, y: e.clientY, actions: [
      { label: t.rename, icon: Icons.Edit, onClick: () => startRename(g.id, g.title) },
      { label: t.settings, icon: Icons.Settings, onClick: () => { store.setEditingProject(g.id); store.setActiveModal('project_edit'); } },
      { separator: true },
      { label: t.delete, icon: Icons.Trash, onClick: () => store.deleteGroup(g.id), danger: true }
    ]});
  };

  const onContextMenuSession = (e: any, s: ChatSession) => {
    e.preventDefault();
    const moveSubMenu = [
      { label: t.ungrouped, onClick: () => store.moveSession(s.id, undefined) },
      ...activeGroups.map(g => ({ label: g.title || t.untitledGroup, onClick: () => store.moveSession(s.id, g.id) }))
    ];

    setContextMenu({ x: e.clientX, y: e.clientY, actions: [
      { label: t.rename, icon: Icons.Edit, onClick: () => startRename(s.id, s.title) },
      { label: t.emailChat, icon: Icons.Mail, onClick: () => alert(`${t.emailChat} to ${store.user?.email || 'user@example.com'}`) },
      { label: t.moveToGroup, icon: Icons.MoveTo, subMenu: moveSubMenu },
      { separator: true },
      { label: t.delete, icon: Icons.Trash, onClick: () => store.deleteSession(s.id), danger: true }
    ]});
  };

  // Helper component for Slim Mode Buttons
  const SlimButton = ({ icon: Icon, label, onClick, children }: any) => (
    <div className="relative group/slim-item w-full flex justify-center py-2">
       <button 
         onClick={onClick}
         className="w-10 h-10 flex items-center justify-center rounded-xl bg-gray-100 hover:bg-black hover:text-white text-gray-500 transition-all active:scale-95 shadow-sm"
       >
          <Icon className="w-5 h-5" />
       </button>
       
       {/* Flyout Menu */}
       {children && (
         <div className="absolute left-full top-0 ml-3 w-64 bg-white border border-gray-100 rounded-2xl shadow-2xl p-2 hidden group-hover/slim-item:block z-50 animate-fadeIn origin-top-left max-h-[80vh] overflow-y-auto custom-scrollbar">
            <div className="px-3 py-2 text-xs font-black text-gray-400 uppercase tracking-widest border-b border-gray-50 mb-1">
                {label}
            </div>
            {children}
         </div>
       )}
    </div>
  );

  return (
    <motion.aside 
      layout
      animate={{ width: isSlim ? 72 : sidebarWidth }}
      transition={{ duration: isResizing ? 0 : 0.35, ease: [0.4, 0, 0.2, 1] }}
      className="flex flex-col bg-[#F9FAFB] border-r border-gray-200 h-full select-none z-10 relative shrink-0"
    >
      {/* Header */}
      <div className="h-16 flex items-center justify-between px-5 shrink-0 overflow-hidden">
        {!isSlim && (
          <motion.div 
            initial={{ opacity: 0, x: -10 }}
            animate={{ opacity: 1, x: 0 }}
            className="flex items-center gap-3 flex-1 overflow-hidden"
          >
            <div className="w-8 h-8 bg-black text-white rounded-xl flex items-center justify-center shadow-lg shrink-0">
              <Icons.Zap className="w-5 h-5" fill="currentColor" />
            </div>
            <span className="font-black text-xl tracking-tighter text-black uppercase truncate">Agent</span>
          </motion.div>
        )}
        
        <button 
            onClick={(e) => { e.stopPropagation(); store.toggleSidebar(); }}
            className={clsx(
              "w-10 h-10 bg-black text-white rounded-xl flex items-center justify-center shadow-lg hover:bg-gray-800 transition-all hover:scale-105 active:scale-95 shrink-0",
              isSlim ? "mx-auto" : ""
            )}
        >
            <motion.div
              animate={{ rotate: isSlim ? 180 : 0 }}
              transition={{ type: "spring", stiffness: 200 }}
            >
              <Icons.ChevronLeft className="w-5 h-5" strokeWidth={3} />
            </motion.div>
        </button>
      </div>

      {/* New Chat Button */}
      <div className={clsx("mb-2 transition-all shrink-0", isSlim ? "px-2" : "px-5")}>
        <button 
          onClick={store.createNewSession} 
          className={clsx(
            "bg-black text-white flex items-center justify-center transition-all hover:bg-gray-800 rounded-xl shadow-xl active:scale-[0.98] border border-white/5",
            isSlim ? "w-11 h-11 mx-auto" : "w-full h-11 gap-2"
          )}
          title={isSlim ? t.newChat : undefined}
        >
          <Icons.Plus className="w-5 h-5" strokeWidth={3} />
          {!isSlim && <span className="text-[11px] font-black tracking-[0.1em] uppercase">{t.newChat}</span>}
        </button>
      </div>

      {/* Search Button - Replaced below New Chat */}
      <div className={clsx("mb-4 transition-all shrink-0", isSlim ? "px-2" : "px-5")}>
         <button 
            onClick={store.toggleSearch}
            className={clsx(
                "flex items-center justify-center transition-all active:scale-[0.98] border border-transparent",
                 isSlim 
                   ? "w-11 h-11 mx-auto rounded-xl hover:bg-gray-200/50 text-gray-500 bg-white shadow-sm" 
                   : "w-full h-10 gap-3 px-4 text-left rounded-xl hover:bg-gray-200/50 text-gray-500 hover:text-black font-bold"
            )}
            title={isSlim ? t.search : undefined}
         >
             <Icons.Search className={clsx("w-5 h-5", !isSlim && "text-gray-400")} />
             {!isSlim && <span>{t.search}</span>}
         </button>
      </div>

      {/* Navigation Content */}
      <div className="flex-1 overflow-hidden relative">
        <AnimatePresence mode="popLayout">
          {!isSlim ? (
            <motion.div 
              key="full-rail"
              initial={{ opacity: 0 }} 
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              className="h-full overflow-y-auto custom-scrollbar space-y-4 pt-2"
            >
                {/* Expanded Projects */}
                <div>
                   <div 
                      onClick={() => setIsProjectsCollapsed(!isProjectsCollapsed)}
                      className="text-[10px] font-black text-gray-400 uppercase tracking-[0.2em] px-4 mb-2 flex justify-between items-center opacity-60 hover:opacity-100 transition-opacity cursor-pointer group/header"
                   >
                      <div className="flex items-center gap-2">
                         <Icons.ChevronDown className={clsx("w-3.5 h-3.5 transition-transform", isProjectsCollapsed ? "-rotate-90" : "rotate-0")} />
                         <span>{t.groups}</span> 
                      </div>
                      <button 
                         onClick={(e) => { e.stopPropagation(); store.createGroup(uuidv4(), t.untitledGroup); }} 
                         className="hover:text-black transition-all p-1 hover:bg-gray-200 rounded-md"
                      >
                          <Icons.Plus className="w-3.5 h-3.5" />
                      </button>
                   </div>
                   <motion.div 
                      initial={false}
                      animate={{ height: isProjectsCollapsed ? 0 : 'auto', opacity: isProjectsCollapsed ? 0 : 1 }}
                      className="overflow-hidden space-y-0.5"
                   >
                      {groupedSessions.map(({ group, sessions }) => (
                        <div key={group.id}>
                            <NavItem 
                              group={group}
                              isEditing={editingId === group.id}
                              editValue={editValue}
                              onEditChange={setEditValue}
                              onEditSubmit={handleRenameSubmit}
                              onContextMenu={(e: any) => onContextMenuGroup(e, group)}
                              onActionClick={(e: any) => onContextMenuGroup(e, group)}
                              onClick={() => store.updateGroup(group.id, { collapsed: !group.collapsed })}
                            >
                              {group.title || t.untitledGroup}
                            </NavItem>
                            {!group.collapsed && (
                               <div className="ml-[20px] border-l-2 border-gray-200 mt-1 pl-3 space-y-0.5 mb-5 pr-1 relative">
                                  {sessions.map(s => (
                                    <NavItem 
                                      key={s.id} 
                                      isSubItem
                                      isActive={s.id === store.currentSessionId} 
                                      isEditing={editingId === s.id}
                                      editValue={editValue}
                                      onEditChange={setEditValue}
                                      onEditSubmit={handleRenameSubmit}
                                      onClick={() => store.selectSession(s.id)} 
                                      onContextMenu={(e: any) => onContextMenuSession(e, s)}
                                      onActionClick={(e: any) => onContextMenuSession(e, s)}
                                    >
                                       {s.title || "Untitled Chat"}
                                    </NavItem>
                                  ))}
                               </div>
                            )}
                        </div>
                      ))}
                   </motion.div>
                </div>

                {/* Expanded Recent */}
                <div>
                   <div 
                      onClick={() => setIsRecentCollapsed(!isRecentCollapsed)}
                      className="text-[10px] font-black text-gray-400 uppercase tracking-[0.2em] px-4 mb-2 flex items-center gap-2 opacity-60 hover:opacity-100 transition-opacity cursor-pointer"
                   >
                      <Icons.ChevronDown className={clsx("w-3.5 h-3.5 transition-transform", isRecentCollapsed ? "-rotate-90" : "rotate-0")} />
                      <span>{t.recent}</span>
                   </div>
                   <motion.div 
                      initial={false}
                      animate={{ height: isRecentCollapsed ? 0 : 'auto', opacity: isRecentCollapsed ? 0 : 1 }}
                      className="overflow-hidden space-y-0.5 relative"
                   >
                      <div className="ml-[20px] border-l-2 border-gray-200 mt-1 pl-3 space-y-0.5 mb-5 pr-1 relative">
                        {ungrouped.map(s => (
                          <NavItem 
                            key={s.id} 
                            isSubItem
                            isActive={s.id === store.currentSessionId} 
                            isEditing={editingId === s.id}
                            editValue={editValue}
                            onEditChange={setEditValue}
                            onEditSubmit={handleRenameSubmit}
                            onClick={() => store.selectSession(s.id)} 
                            onContextMenu={(e: any) => onContextMenuSession(e, s)}
                            onActionClick={(e: any) => onContextMenuSession(e, s)}
                          >
                             {s.title || "Untitled Chat"}
                          </NavItem>
                        ))}
                      </div>
                   </motion.div>
                </div>
            </motion.div>
          ) : (
            <motion.div 
              key="slim-rail"
              initial={{ opacity: 0 }} 
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              className="h-full flex flex-col items-center gap-4 pt-4 overflow-visible"
            >
               {/* 1. Projects Button with Flyout */}
               <SlimButton icon={Icons.Layers} label={t.groups} onClick={() => store.toggleSidebar()}>
                  {activeGroups.length > 0 ? (
                      <div className="space-y-1">
                          {activeGroups.map(g => (
                              <button
                                key={g.id}
                                onClick={(e) => { e.stopPropagation(); store.toggleSidebar(); }}
                                className="w-full flex items-center gap-3 p-2 rounded-lg hover:bg-gray-50 transition-colors text-left group/item"
                              >
                                  <ProjectMarker group={g} />
                                  <span className="text-sm font-bold text-gray-700 truncate flex-1">{g.title || t.untitledGroup}</span>
                              </button>
                          ))}
                      </div>
                  ) : (
                      <div className="p-2 text-xs text-gray-400 text-center italic">No projects</div>
                  )}
               </SlimButton>

               {/* 2. Chats Button with Flyout */}
               <SlimButton icon={Icons.History} label={t.chats} onClick={() => store.toggleSidebar()}>
                  {ungrouped.length > 0 ? (
                      <div className="space-y-1">
                         {ungrouped.slice(0, 10).map(s => (
                            <button
                                key={s.id}
                                onClick={(e) => { e.stopPropagation(); store.selectSession(s.id); }}
                                className={clsx(
                                    "w-full text-left p-2 rounded-lg text-xs font-bold truncate transition-colors",
                                    s.id === store.currentSessionId ? "bg-black text-white" : "text-gray-600 hover:bg-gray-100"
                                )}
                            >
                                {s.title || "Untitled Chat"}
                            </button>
                         ))}
                      </div>
                  ) : (
                      <div className="p-2 text-xs text-gray-400 text-center italic">No chats</div>
                  )}
               </SlimButton>

               {/* Removed Search from bottom, added to top */}
            </motion.div>
          )}
        </AnimatePresence>
      </div>

      {/* User Section */}
      <div className="p-2 border-t border-gray-200 bg-[#F9FAFB] shrink-0 relative" ref={userMenuRef}>
         <AnimatePresence>
         {showUserMenu && (
           <motion.div 
             initial={{ opacity: 0, x: isSlim ? 10 : 0, y: 10, scale: 0.95 }} 
             animate={{ opacity: 1, x: 0, y: 0, scale: 1 }} 
             exit={{ opacity: 0, x: isSlim ? 10 : 0, y: 10, scale: 0.95 }}
             className={clsx(
                "absolute bottom-full bg-white rounded-[22px] shadow-[0_15px_45px_-12px_rgba(0,0,0,0.15)] border border-gray-200 py-1.5 mb-3 z-[100] text-[13px] overflow-hidden whitespace-nowrap",
                isSlim ? "left-14 w-52" : "left-2.5 right-2.5"
             )}
           >
              <div className="px-4 py-2 mb-1 border-b border-gray-50 bg-gray-50/50">
                  <div className="font-black text-black text-xs truncate">{store.user?.name}</div>
                  <div className="text-[10px] text-gray-400 font-bold truncate mt-0.5">{store.user?.email}</div>
              </div>
              <button onClick={() => { setShowUserMenu(false); store.setActiveModal('account'); }} className="w-full px-4 py-2 text-left hover:bg-gray-50 flex items-center gap-2.5 font-bold text-gray-700 transition-colors">
                <Icons.User className="w-3.5 h-3.5 text-gray-400" /> 
                <span>{t.account}</span>
              </button>
              <button onClick={() => { setShowUserMenu(false); store.setActiveModal('settings'); }} className="w-full px-4 py-2 text-left hover:bg-gray-50 flex items-center gap-2.5 font-bold text-gray-700 transition-colors">
                <Icons.Settings className="w-3.5 h-3.5 text-gray-400" /> 
                <span>{t.settings}</span>
              </button>
              <button onClick={() => { setShowUserMenu(false); store.setActiveModal('help'); }} className="w-full px-4 py-2 text-left hover:bg-gray-50 flex items-center gap-2.5 font-bold text-gray-700 transition-colors">
                <Icons.Help className="w-3.5 h-3.5 text-gray-400" /> 
                <span>{t.getHelp}</span>
              </button>
              <div className="h-px bg-gray-100 my-1 mx-4" />
              <button onClick={() => { setShowUserMenu(false); store.setActiveModal('upgrade'); }} className="w-full px-4 py-2 text-left hover:bg-yellow-50 flex items-center gap-2.5 font-bold text-yellow-700 transition-colors">
                <Icons.CreditCard className="w-3.5 h-3.5 text-yellow-500" /> 
                <span>{t.upgradeSubscription}</span>
              </button>
              <div className="h-px bg-gray-100 my-1 mx-4" />
              <button onClick={() => { setShowUserMenu(false); store.logout(); }} className="w-full px-4 py-2 text-left hover:bg-red-50 text-red-600 flex items-center gap-2.5 font-bold transition-colors">
                <Icons.LogOut className="w-3.5 h-3.5" /> 
                <span>{t.signOut}</span>
              </button>
           </motion.div>
         )}
         </AnimatePresence>
         
         <button 
           onClick={(e) => {
              e.stopPropagation();
              setShowUserMenu(!showUserMenu);
           }} 
           className={clsx(
             "flex items-center gap-2.5 p-1.5 rounded-xl transition-all hover:bg-gray-200/50 w-full active:scale-[0.98]",
             isSlim ? "justify-center" : ""
           )}
         >
            <div className="w-11 h-11 md:w-8 md:h-8 rounded-full bg-gradient-to-tr from-gray-200 to-gray-300 flex items-center justify-center text-gray-600 shadow-sm border-2 border-white shrink-0 relative overflow-hidden transition-all">
              {store.user?.avatar ? (
                  <img src={store.user.avatar} className="w-full h-full object-cover filter grayscale" />
              ) : (
                  <Icons.User className="w-4.5 h-4.5" />
              )}
            </div>
            {!isSlim && (
              <div className="flex-1 text-left overflow-hidden">
                <div className="text-[12px] font-bold text-black truncate leading-tight">{store.user?.name || "Agent 用户"}</div>
                <div className="text-[9px] text-gray-400 font-bold truncate leading-none mt-0.5">{store.user?.email}</div>
              </div>
            )}
         </button>
      </div>

      {/* Resizing Handle */}
      {!isSlim && (
        <div 
          onMouseDown={() => setIsResizing(true)}
          className="absolute right-0 top-0 bottom-0 w-1 hover:w-1.5 bg-transparent hover:bg-black/10 transition-all cursor-col-resize z-20"
        />
      )}

      {contextMenu && <ContextMenu {...contextMenu} onClose={() => setContextMenu(null)} />}
    </motion.aside>
  );
};
