
import React, { useState, useRef, useEffect } from 'react';
import { useStore } from '../store';
import { Icons } from './icons';
import clsx from 'clsx';
import { AnimatePresence, motion } from 'framer-motion';
import { translations } from '../locales';
import { ChatSession, Group } from '../types';
import { v4 as uuidv4 } from 'uuid';

// --- Components for Sidebar Items ---

const ContextMenu = ({ 
  x, 
  y, 
  onClose, 
  actions 
}: { 
  x: number; 
  y: number; 
  onClose: () => void; 
  actions: { label: string; icon?: any; onClick?: () => void; danger?: boolean; separator?: boolean; subMenu?: any[] }[] 
}) => {
  const menuRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) {
        onClose();
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, [onClose]);

  // Adjust position to keep in viewport
  const style: React.CSSProperties = {
    top: y,
    left: x,
  };

  // Simple viewport check
  if (y + 300 > window.innerHeight) style.top = y - 200;

  return (
    <div 
      ref={menuRef}
      className="fixed z-50 w-48 bg-white rounded-lg shadow-xl border border-gray-200 py-1 text-sm animate-fadeIn select-none"
      style={style}
    >
      {actions.map((action, idx) => (
        <React.Fragment key={idx}>
          {action.separator && <div className="h-px bg-gray-100 my-1" />}
          {action.subMenu ? (
              <div className="relative group/submenu">
                 <button className="w-full px-3 py-2 text-left flex items-center gap-2 hover:bg-gray-50 text-gray-700">
                    {action.icon && <action.icon className="w-4 h-4 text-gray-400" />}
                    <span className="flex-1">{action.label}</span>
                    <Icons.ChevronRight className="w-3 h-3 text-gray-400" />
                 </button>
                 <div className="absolute left-full top-0 ml-1 w-40 bg-white rounded-lg shadow-xl border border-gray-200 py-1 hidden group-hover/submenu:block max-h-64 overflow-y-auto">
                     {action.subMenu.map((sub, subIdx) => (
                         <button
                            key={subIdx}
                            onClick={() => { sub.onClick(); onClose(); }}
                            className="w-full px-3 py-2 text-left hover:bg-gray-50 text-gray-700 text-xs truncate"
                         >
                             {sub.label}
                         </button>
                     ))}
                     {action.subMenu.length === 0 && (
                        <div className="px-3 py-2 text-xs text-gray-400 italic">No options</div>
                     )}
                 </div>
              </div>
          ) : (
              action.label ? (
                <button
                    onClick={() => { action.onClick?.(); onClose(); }}
                    className={clsx(
                    "w-full px-3 py-2 text-left flex items-center gap-2 hover:bg-gray-50",
                    action.danger ? "text-red-600 hover:bg-red-50" : "text-gray-700"
                    )}
                >
                    {action.icon && <action.icon className={clsx("w-4 h-4", action.danger ? "text-red-500" : "text-gray-400")} />}
                    <span>{action.label}</span>
                </button>
              ) : null
          )}
        </React.Fragment>
      ))}
    </div>
  );
};

const SessionItem = ({ 
  session, 
  isActive, 
  onSelect, 
  onContextMenu,
  isEditing,
  editValue,
  setEditValue,
  onEditSubmit
}: { 
  session: ChatSession; 
  isActive: boolean; 
  onSelect: () => void; 
  onContextMenu: (e: React.MouseEvent, session: ChatSession) => void;
  isEditing: boolean;
  editValue: string;
  setEditValue: (v: string) => void;
  onEditSubmit: () => void;
}) => {
  return (
    <div className="group relative">
        {isEditing ? (
            <div className="px-2 py-1">
                 <input
                    autoFocus
                    type="text"
                    value={editValue}
                    onChange={(e) => setEditValue(e.target.value)}
                    onBlur={onEditSubmit}
                    onKeyDown={(e) => e.key === 'Enter' && onEditSubmit()}
                    className="w-full px-2 py-1.5 text-sm border border-blue-500 rounded bg-white shadow-sm text-black focus:outline-none"
                />
            </div>
        ) : (
            <>
                <button
                    onClick={onSelect}
                    className={clsx(
                    "w-full text-left px-3 py-2.5 rounded-lg text-sm transition-all duration-200 relative overflow-hidden",
                    isActive 
                        ? "bg-white text-black shadow-sm border border-gray-200 font-medium" 
                        : "text-gray-600 hover:bg-gray-200/50 hover:text-black"
                    )}
                    onContextMenu={(e) => onContextMenu(e, session)}
                >
                    <div className="flex items-center gap-3 relative z-10">
                    <Icons.Chat className={clsx("w-4 h-4 shrink-0", isActive ? "text-black" : "text-gray-400")} />
                    <span className="truncate flex-1 pr-6">{session.title}</span>
                    </div>
                    {isActive && (
                        <div className="absolute left-0 top-1/2 -translate-y-1/2 w-1 h-5 bg-black rounded-r-full" />
                    )}
                </button>
                {/* More Button (visible on hover) */}
                <button 
                    onClick={(e) => onContextMenu(e, session)}
                    className={clsx(
                        "absolute right-2 top-1/2 -translate-y-1/2 p-1 rounded-md text-gray-400 hover:text-black hover:bg-gray-200 transition-opacity z-20",
                        isActive ? "opacity-100" : "opacity-0 group-hover:opacity-100"
                    )}
                >
                    <Icons.More className="w-4 h-4" />
                </button>
            </>
        )}
    </div>
  );
};

const GroupItem = ({
  group,
  sessions,
  currentSessionId,
  onSelectSession,
  onToggleCollapse,
  onContextMenuSession,
  onContextMenuGroup,
  editingId,
  editValue,
  setEditValue,
  onEditSubmit
}: {
  group: Group;
  sessions: ChatSession[];
  currentSessionId: string | null;
  onSelectSession: (id: string) => void;
  onToggleCollapse: (id: string) => void;
  onContextMenuSession: (e: React.MouseEvent, session: ChatSession) => void;
  onContextMenuGroup: (e: React.MouseEvent, group: Group) => void;
  editingId: string | null;
  editValue: string;
  setEditValue: (v: string) => void;
  onEditSubmit: () => void;
}) => {
    const isEditingGroup = editingId === group.id;

    return (
        <div className="mb-2">
            {isEditingGroup ? (
                <div className="px-2 py-1">
                    <input
                        autoFocus
                        type="text"
                        value={editValue}
                        onChange={(e) => setEditValue(e.target.value)}
                        onBlur={onEditSubmit}
                        onKeyDown={(e) => e.key === 'Enter' && onEditSubmit()}
                        className="w-full px-2 py-1.5 text-xs border border-blue-500 rounded bg-white shadow-sm text-black focus:outline-none font-semibold"
                    />
                </div>
            ) : (
                <div 
                    className="flex items-center justify-between px-3 py-1.5 text-xs font-semibold text-gray-500 hover:text-black group cursor-pointer select-none"
                    onContextMenu={(e) => onContextMenuGroup(e, group)}
                >
                    <div className="flex items-center gap-1 flex-1" onClick={() => onToggleCollapse(group.id)}>
                        {group.collapsed ? <Icons.ChevronRight className="w-3 h-3" /> : <Icons.ChevronDown className="w-3 h-3" />}
                        <Icons.Folder className={clsx("w-3 h-3 ml-1", sessions.length > 0 ? "text-gray-600" : "text-gray-400")} />
                        <span className="truncate ml-1">{group.title}</span>
                        <span className="text-gray-400 font-normal ml-1">({sessions.length})</span>
                    </div>
                    <button 
                        onClick={(e) => onContextMenuGroup(e, group)}
                        className="opacity-0 group-hover:opacity-100 p-0.5 hover:bg-gray-200 rounded text-gray-400 hover:text-black"
                    >
                        <Icons.More className="w-3 h-3" />
                    </button>
                </div>
            )}
            
            {!group.collapsed && (
                <div className="space-y-0.5 ml-4 pl-2 border-l border-gray-200/50">
                    {sessions.map(session => (
                         <SessionItem 
                            key={session.id}
                            session={session}
                            isActive={session.id === currentSessionId}
                            onSelect={() => onSelectSession(session.id)}
                            onContextMenu={onContextMenuSession}
                            isEditing={editingId === session.id}
                            editValue={editValue}
                            setEditValue={setEditValue}
                            onEditSubmit={onEditSubmit}
                         />
                    ))}
                    {sessions.length === 0 && (
                        <div className="px-3 py-2 text-xs text-gray-400 italic ml-1">Empty</div>
                    )}
                </div>
            )}
        </div>
    );
};

// --- Main Sidebar Component ---

export const Sidebar: React.FC = () => {
  const store = useStore();
  const t = translations[store.language];
  const setActiveModal = useStore(s => s.setActiveModal);
  const logout = useStore(s => s.logout);

  const [contextMenu, setContextMenu] = useState<{
    type: 'session' | 'group';
    item: ChatSession | Group;
    x: number;
    y: number;
  } | null>(null);

  const [editingId, setEditingId] = useState<string | null>(null);
  const [editValue, setEditValue] = useState('');
  const sidebarRef = useRef<HTMLDivElement>(null);
  
  const [showUserMenu, setShowUserMenu] = useState(false);
  const userMenuRef = useRef<HTMLDivElement>(null);

  // Group logic
  const groupedSessions = store.groups.map(group => ({
      group,
      sessions: store.sessions.filter(s => s.groupId === group.id)
  }));
  const ungroupedSessions = store.sessions.filter(s => !s.groupId);

  // Handlers
  const handleCreateGroup = () => {
      if (store.groups.length >= 10) {
          alert(t.groupLimitReached);
          return;
      }
      const newId = uuidv4();
      store.createGroup(newId, t.untitledGroup);
      
      // Immediately enter rename mode
      setEditingId(newId);
      setEditValue(t.untitledGroup);
  };

  const handleContextMenuSession = (e: React.MouseEvent, session: ChatSession) => {
      e.preventDefault();
      e.stopPropagation();
      setContextMenu({ type: 'session', item: session, x: e.clientX, y: e.clientY });
  };

  const handleContextMenuGroup = (e: React.MouseEvent, group: Group) => {
      e.preventDefault();
      e.stopPropagation();
      setContextMenu({ type: 'group', item: group, x: e.clientX, y: e.clientY });
  };

  const handleRenameStart = (id: string, currentTitle: string) => {
      setEditingId(id);
      setEditValue(currentTitle);
  };

  const handleRenameSubmit = () => {
      if (!editingId) return;
      if (editValue.trim()) {
          // Check if it's a group or session
          const isGroup = store.groups.some(g => g.id === editingId);
          if (isGroup) {
              store.updateGroup(editingId, { title: editValue.trim() });
          } else {
              store.renameSession(editingId, editValue.trim());
          }
      }
      setEditingId(null);
  };

  const handleEmailSession = (session: ChatSession) => {
      const body = session.messages.map(m => `[${m.role}]: ${m.content}`).join('\n\n');
      const subject = `Agent Chat: ${session.title}`;
      const mailtoLink = `mailto:?subject=${encodeURIComponent(subject)}&body=${encodeURIComponent(body)}`;
      window.open(mailtoLink, '_blank');
  };

  const handleSignOut = () => {
    setShowUserMenu(false);
    logout();
  };

  // Close context menu on global click
  useEffect(() => {
      const closeMenu = () => setContextMenu(null);
      const closeUserMenu = (e: MouseEvent) => {
        if (userMenuRef.current && !userMenuRef.current.contains(e.target as Node)) {
          setShowUserMenu(false);
        }
      };
      window.addEventListener('click', closeMenu);
      window.addEventListener('mousedown', closeUserMenu);
      return () => {
        window.removeEventListener('click', closeMenu);
        window.removeEventListener('mousedown', closeUserMenu);
      };
  }, []);

  // Ensure we have a selected session if none
  useEffect(() => {
    if (!store.currentSessionId && store.sessions.length > 0) {
      store.selectSession(store.sessions[0].id);
    }
  }, [store.currentSessionId, store.sessions, store.selectSession]);

  // Build Actions for Context Menu
  const getContextMenuActions = () => {
      if (!contextMenu) return [];
      
      if (contextMenu.type === 'session') {
          const session = contextMenu.item as ChatSession;
          
          // Submenu for Move To Group
          const moveSubMenu = [
              { 
                  label: t.ungrouped, 
                  onClick: () => store.moveSession(session.id, undefined) 
              },
              ...store.groups.map(g => ({
                  label: g.title,
                  onClick: () => store.moveSession(session.id, g.id)
              }))
          ];

          return [
              { label: t.rename, icon: Icons.Edit, onClick: () => handleRenameStart(session.id, session.title) },
              { label: t.emailChat, icon: Icons.Mail, onClick: () => handleEmailSession(session) },
              { label: t.moveToGroup, icon: Icons.MoveTo, subMenu: moveSubMenu },
              { separator: true, label: '', onClick: () => {} },
              { label: t.delete, icon: Icons.Trash, onClick: () => store.deleteSession(session.id), danger: true }
          ];
      } else {
          const group = contextMenu.item as Group;
          return [
              { label: t.rename, icon: Icons.Edit, onClick: () => handleRenameStart(group.id, group.title) },
              { separator: true, label: '', onClick: () => {} },
              { label: t.delete, icon: Icons.Trash, onClick: () => store.deleteGroup(group.id), danger: true }
          ];
      }
  };

  return (
    <>
    <AnimatePresence>
      {store.isSidebarOpen && (
        <motion.div 
          initial={{ width: 0, opacity: 0 }}
          animate={{ width: 280, opacity: 1 }}
          exit={{ width: 0, opacity: 0 }}
          transition={{ duration: 0.3, ease: [0.4, 0, 0.2, 1] }}
          className="hidden md:flex flex-col bg-[#F9FAFB] border-r border-gray-200 h-full shrink-0 overflow-hidden whitespace-nowrap select-none relative"
          ref={sidebarRef}
        >
          {/* App Logo & Header */}
          <div className="px-4 pt-4 pb-2">
              <button 
                onClick={store.navigateToHome} 
                className="flex items-center gap-2 group w-full text-left outline-none mb-4"
              >
                  <div className="w-8 h-8 bg-black text-white rounded-lg flex items-center justify-center shadow-sm group-hover:bg-gray-800 transition-colors">
                     <Icons.Zap className="w-5 h-5" fill="currentColor" />
                  </div>
                  <span className="text-xl font-bold text-gray-900 tracking-tight">Agent</span>
              </button>

              {/* Prominent New Chat Button */}
              <button 
                onClick={store.createNewSession}
                className="w-full flex items-center justify-center gap-2 bg-black hover:bg-gray-800 text-white py-2.5 rounded-lg shadow-md transition-all duration-200 font-medium text-sm mb-2 transform hover:scale-[1.02]"
              >
                <Icons.NewChat className="w-4 h-4" />
                <span>{t.newChat}</span>
              </button>
          </div>

          {/* Secondary Actions Header */}
          <div className="flex items-center justify-between px-4 py-1 min-w-[280px] mb-2">
             <div className="flex gap-2 w-full">
                <button 
                    onClick={handleCreateGroup}
                    className="flex-1 flex items-center justify-center gap-2 text-xs font-medium text-gray-600 hover:text-black transition-colors bg-white border border-gray-200 px-3 py-1.5 rounded-md shadow-sm hover:shadow group"
                    title={t.createGroup}
                >
                    <Icons.NewGroup className="w-3 h-3 text-gray-400 group-hover:text-black" />
                    <span>{t.createGroup}</span>
                </button>
             </div>
          </div>

          {/* Main List */}
          <div className="flex-1 overflow-y-auto px-3 min-w-[280px] pb-4 custom-scrollbar">
             
             {/* Groups */}
             <div className="mb-4">
                 <div className="text-[11px] font-bold text-gray-400 uppercase tracking-wider px-3 mb-2 flex justify-between items-center">
                    <span>{t.groups}</span>
                    <span className="bg-gray-200 text-gray-500 px-1.5 py-0.5 rounded-full text-[9px]">{store.groups.length}/10</span>
                 </div>
                 {groupedSessions.map(({ group, sessions }) => (
                     <GroupItem
                        key={group.id}
                        group={group}
                        sessions={sessions}
                        currentSessionId={store.currentSessionId}
                        onSelectSession={store.selectSession}
                        onToggleCollapse={(id) => store.updateGroup(id, { collapsed: !group.collapsed })}
                        onContextMenuSession={handleContextMenuSession}
                        onContextMenuGroup={handleContextMenuGroup}
                        editingId={editingId}
                        editValue={editValue}
                        setEditValue={setEditValue}
                        onEditSubmit={handleRenameSubmit}
                     />
                 ))}
             </div>

             {/* Ungrouped / Recent */}
             <div>
                <div className="text-[11px] font-bold text-gray-400 uppercase tracking-wider px-3 mb-2">
                    {t.recent}
                </div>
                <div className="space-y-0.5">
                    {ungroupedSessions.map(session => (
                        <SessionItem
                            key={session.id}
                            session={session}
                            isActive={session.id === store.currentSessionId}
                            onSelect={() => store.selectSession(session.id)}
                            onContextMenu={handleContextMenuSession}
                            isEditing={editingId === session.id}
                            editValue={editValue}
                            setEditValue={setEditValue}
                            onEditSubmit={handleRenameSubmit}
                        />
                    ))}
                    {ungroupedSessions.length === 0 && groupedSessions.length === 0 && (
                         <div className="px-3 py-4 text-center text-sm text-gray-400">
                            No chats yet.
                         </div>
                    )}
                </div>
             </div>
          </div>

          {/* Footer */}
          <div className="p-4 border-t border-gray-200 min-w-[280px] bg-[#F9FAFB] relative" ref={userMenuRef}>
            
            {/* User Menu Popup */}
            {showUserMenu && (
                <motion.div 
                    initial={{ opacity: 0, y: 10 }}
                    animate={{ opacity: 1, y: 0 }}
                    exit={{ opacity: 0, y: 10 }}
                    className="absolute bottom-[110%] left-4 right-4 bg-white rounded-xl shadow-2xl border border-gray-200 py-1.5 z-50 flex flex-col"
                >
                    <button onClick={() => { setShowUserMenu(false); setActiveModal('upgrade'); }} className="flex items-center gap-3 px-3 py-2 hover:bg-gray-50 text-sm text-gray-700 transition-colors">
                        <Icons.CreditCard className="w-4 h-4 text-black" />
                        <span className="font-medium text-black">{t.upgradeSubscription}</span>
                    </button>
                    <button onClick={() => { setShowUserMenu(false); setActiveModal('account'); }} className="flex items-center gap-3 px-3 py-2 hover:bg-gray-50 text-sm text-gray-700 transition-colors">
                        <Icons.User className="w-4 h-4 text-gray-500" />
                        <span>{t.account}</span>
                    </button>
                    <button onClick={() => { setShowUserMenu(false); setActiveModal('help'); }} className="flex items-center gap-3 px-3 py-2 hover:bg-gray-50 text-sm text-gray-700 transition-colors">
                        <Icons.Help className="w-4 h-4 text-gray-500" />
                        <span>{t.getHelp}</span>
                    </button>
                    <button onClick={() => { setShowUserMenu(false); setActiveModal('settings'); }} className="flex items-center gap-3 px-3 py-2 hover:bg-gray-50 text-sm text-gray-700 transition-colors">
                        <Icons.Settings className="w-4 h-4 text-gray-500" />
                        <span>{t.settings}</span>
                    </button>
                    <div className="h-px bg-gray-100 my-1" />
                    <button onClick={handleSignOut} className="flex items-center gap-3 px-3 py-2 hover:bg-red-50 text-sm text-red-600 transition-colors">
                        <Icons.LogOut className="w-4 h-4" />
                        <span>{t.signOut}</span>
                    </button>
                </motion.div>
            )}

            {/* Language Toggle */}
             <div className="flex bg-gray-200 rounded-lg p-1 mb-3 relative">
                 <div 
                    className="absolute top-1 bottom-1 bg-white rounded-md shadow-sm transition-all duration-200 ease-out z-0"
                    style={{
                        left: store.language === 'en' ? '4px' : '50%',
                        width: 'calc(50% - 4px)'
                    }}
                 />
                 <button 
                    onClick={() => store.setLanguage('en')}
                    className={clsx("flex-1 text-xs font-medium text-center py-1.5 relative z-10 transition-colors", store.language === 'en' ? "text-black" : "text-gray-500")}
                 >
                    English
                 </button>
                 <button 
                    onClick={() => store.setLanguage('zh')}
                    className={clsx("flex-1 text-xs font-medium text-center py-1.5 relative z-10 transition-colors", store.language === 'zh' ? "text-black" : "text-gray-500")}
                 >
                    中文
                 </button>
             </div>

             <div 
                className="flex items-center gap-3 px-2 py-2 rounded-lg hover:bg-gray-100 cursor-pointer transition-colors group select-none"
                onClick={() => setShowUserMenu(!showUserMenu)}
             >
                <div className="w-8 h-8 rounded-full bg-gradient-to-tr from-gray-200 to-gray-300 flex items-center justify-center text-gray-600 group-hover:text-black">
                   <Icons.Sparkles className="w-4 h-4" />
                </div>
                <div className="flex-1">
                   <div className="text-sm font-medium text-gray-900">Agent User</div>
                   <div className="text-xs text-gray-500">{t.proPlan}</div>
                </div>
                <Icons.MoreVertical className="w-4 h-4 text-gray-400 group-hover:text-black" />
             </div>
          </div>
        </motion.div>
      )}
    </AnimatePresence>

    {/* Context Menu Portal */}
    {contextMenu && (
        <ContextMenu 
            x={contextMenu.x} 
            y={contextMenu.y} 
            onClose={() => setContextMenu(null)} 
            actions={getContextMenuActions()} 
        />
    )}
    </>
  );
};
