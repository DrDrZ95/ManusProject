
import React, { useState, useRef, useEffect, useMemo, useCallback, memo, Profiler } from 'react';
import { useStore } from '../store';
import { Icons } from './icons';
import clsx from 'clsx';
import { AnimatePresence, motion } from 'framer-motion';
import { translations } from '../locales';
import { ChatSession, Group } from '../types';
import { v4 as uuidv4 } from 'uuid';
import { List } from 'react-window';
import { getUserSessionHistory } from '../services/openapi/endpoints/user';
import { OpenClaw } from '@lobehub/icons';

// Custom AutoSizer to avoid dependency issues
const AutoSizer = ({ children }: { children: (size: { width: number, height: number }) => React.ReactNode }) => {
  const ref = useRef<HTMLDivElement>(null);
  const [size, setSize] = useState({ width: 0, height: 0 });

  useEffect(() => {
    if (!ref.current) return;
    const observer = new ResizeObserver((entries) => {
      for (let entry of entries) {
        // Use contentRect for precise dimensions
        setSize({ width: entry.contentRect.width, height: entry.contentRect.height });
      }
    });
    observer.observe(ref.current);
    return () => observer.disconnect();
  }, []);

  return (
    <div ref={ref} style={{ width: '100%', height: '100%', overflow: 'hidden' }}>
      {size.width > 0 && size.height > 0 && children(size)}
    </div>
  );
};

// Performance monitoring callback
const onRenderCallback = (
  id: string,
  phase: "mount" | "update",
  actualDuration: number,
  baseDuration: number,
  startTime: number,
  commitTime: number
) => {
  // Uncomment to debug performance
  // console.log(`[Profiler] ${id} (${phase}) took ${actualDuration.toFixed(2)}ms`);
};

const ContextMenu = memo(({ x, y, onClose, actions }: any) => {
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
      className="fixed z-[100] w-56 bg-white/95 dark:bg-[#1E1F20]/95 backdrop-blur-xl rounded-2xl shadow-2xl border border-gray-200/50 dark:border-[#444746]/50 py-2 text-sm animate-fadeIn"
      style={{ top: Math.min(y, window.innerHeight - 300), left: Math.min(x, window.innerWidth - 240) }}
    >
      {actions.filter(Boolean).map((action: any, idx: number) => {
        if (action.separator) {
          return <div key={idx} className="h-px bg-gray-100 dark:bg-[#444746] my-1.5 mx-2" />;
        }

        if (action.subMenu) {
          return (
            <div key={idx} className="relative group/submenu">
              <button className="w-full px-4 py-2.5 text-left flex items-center gap-3 hover:bg-gray-50 dark:hover:bg-[#333537] text-gray-700 dark:text-[#E3E3E3] font-bold transition-colors">
                {action.icon && <action.icon className="w-4 h-4 text-gray-400 dark:text-[#C4C7C5]" />}
                <span className="flex-1">{action.label}</span>
                <Icons.ChevronRight className="w-3.5 h-3.5 text-gray-400 dark:text-[#C4C7C5]" />
              </button>
              <div className="absolute left-full top-0 -ml-1 w-48 bg-white dark:bg-[#1E1F20] rounded-2xl shadow-2xl border border-gray-200 dark:border-[#444746] py-1.5 hidden group-hover/submenu:block">
                {action.subMenu.filter(Boolean).map((sub: any, sidx: number) => (
                  <button 
                    key={sidx} 
                    onClick={() => { sub.onClick(); onClose(); }} 
                    className="w-full px-4 py-2 text-left hover:bg-gray-50 dark:hover:bg-[#333537] text-[11px] font-black truncate text-gray-500 dark:text-[#C4C7C5] hover:text-black dark:hover:text-white transition-colors"
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
              action.danger ? "text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20" : "text-gray-700 dark:text-[#E3E3E3] hover:bg-gray-50 dark:hover:bg-[#333537]"
            )}
          >
            {action.icon && <action.icon className={clsx("w-4 h-4", action.danger ? "text-red-500" : "text-gray-400 dark:text-[#C4C7C5]")} />}
            <span>{action.label}</span>
          </button>
        );
      })}
    </div>
  );
});

const ProjectMarker = memo(({ group, isSlim }: { group: Group, isSlim?: boolean }) => (
  <div className={clsx(
    "flex-shrink-0 rounded-lg flex items-center justify-center font-black text-white shadow-sm ring-1 ring-black/5 transition-transform group-hover/item:scale-105",
    isSlim ? "w-8 h-8 text-xs" : "w-6 h-6 text-[10px]",
    group.color
  )}>
    {group.marker.charAt(0)}
  </div>
));

interface NavItemProps {
    isActive?: boolean;
    isEditing?: boolean;
    editValue?: string;
    onEditChange?: (val: string) => void;
    onEditSubmit?: (cancel?: boolean) => void;
    onClick: () => void;
    onContextMenu?: (e: React.MouseEvent) => void;
    onActionClick?: (e: React.MouseEvent) => void;
    children?: React.ReactNode;
    icon?: any;
    group?: Group;
    isSlim?: boolean;
    isSubItem?: boolean;
    draggable?: boolean;
    onDragStart?: (e: React.DragEvent) => void;
    onDrop?: (e: React.DragEvent) => void;
    style?: React.CSSProperties; // Added for react-window
}

const NavItem: React.FC<NavItemProps> = memo(({ 
    isActive, isEditing, editValue, onEditChange, onEditSubmit, onClick, onContextMenu, onActionClick, children, icon: Icon, group, isSlim, isSubItem,
    draggable, onDragStart, onDrop, style
}) => {
    const [isDragOver, setIsDragOver] = useState(false);

    const handleDragOver = useCallback((e: React.DragEvent) => {
        if (onDrop) {
            e.preventDefault();
            setIsDragOver(true);
        }
    }, [onDrop]);

    const handleDragLeave = useCallback((e: React.DragEvent) => {
        if (onDrop) {
            setIsDragOver(false);
        }
    }, [onDrop]);

    const handleDrop = useCallback((e: React.DragEvent) => {
        if (onDrop) {
            e.preventDefault();
            setIsDragOver(false);
            onDrop(e);
        }
    }, [onDrop]);

    return (
      <div style={style} className={clsx("relative group/item mb-0.5", isSlim ? "px-2" : isSubItem ? "pr-2" : "px-4")}>
        <button
          onClick={onClick}
          onContextMenu={onContextMenu}
          title={isSlim ? (children as string) : undefined}
          draggable={draggable}
          onDragStart={onDragStart}
          onDragOver={handleDragOver}
          onDragLeave={handleDragLeave}
          onDrop={handleDrop}
          className={clsx(
            "w-full h-10 flex items-center rounded-xl transition-all duration-200 relative border border-transparent",
            isSlim ? "justify-center px-0" : "px-2 gap-3",
            isActive ? "bg-gray-200/50 dark:bg-[#004A77] text-black dark:text-[#E3E3E3] font-black" : "text-gray-600 dark:text-[#C4C7C5] hover:bg-gray-200/30 dark:hover:bg-[#333537]",
            isSubItem && !isSlim && "h-9",
            isDragOver && "bg-blue-50 dark:bg-blue-900/20 border-blue-300 dark:border-blue-700 shadow-inner"
          )}
        >
          {group ? (
            <ProjectMarker group={group} isSlim={isSlim} />
          ) : Icon ? (
            <Icon className={clsx(isSlim ? "w-5 h-5" : "w-4 h-4", "shrink-0", isActive ? "text-black dark:text-[#E3E3E3]" : "text-gray-400 dark:text-[#C4C7C5] group-hover:text-black dark:group-hover:text-white")} />
          ) : (
            isSlim ? (
              <div className={clsx("w-8 h-8 rounded-lg flex items-center justify-center transition-colors", isActive ? "bg-black dark:bg-white text-white dark:text-black" : "bg-gray-100 dark:bg-[#333537] text-gray-400 dark:text-[#C4C7C5]")}>
                <Icons.Chat className="w-4 h-4" />
              </div>
            ) : null
          )}
          
          {!isSlim && (
            <div className="flex-1 min-w-0 pr-1 flex items-center justify-between">
              {isEditing ? (
                <input 
                  autoFocus 
                  className="w-full bg-transparent outline-none text-sm font-black border-b-2 border-black/10 dark:border-white/10 focus:border-black dark:focus:border-white py-0.5 text-black dark:text-[#E3E3E3]"
                  value={editValue}
                  onChange={(e) => onEditChange?.(e.target.value)}
                  onBlur={() => onEditSubmit?.()}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter') onEditSubmit?.();
                    if (e.key === 'Escape') onEditSubmit?.(true);
                  }}
                  onClick={(e) => e.stopPropagation()}
                />
              ) : (
                <span className={clsx("block truncate text-[13px] tracking-tight transition-opacity text-left flex-1", isActive ? "opacity-100" : "opacity-80 font-bold")}>
                    {children || "Untitled"}
                </span>
              )}
              
              {/* Expand/Collapse Icon moved to the right */}
              {group && !isEditing && (
                  <div className="text-gray-400 dark:text-[#C4C7C5] transition-transform duration-200 ml-2">
                       {group.collapsed ? <Icons.ChevronRight className="w-4 h-4" /> : <Icons.ChevronDown className="w-4 h-4" />}
                  </div>
              )}
            </div>
          )}
    
          {isActive && !isEditing && (
            <div className={clsx(
              "absolute top-1/2 -translate-y-1/2 w-1 h-4 bg-black dark:bg-[#A8C7FA] rounded-r-full shadow-sm shadow-black/20 dark:shadow-white/20",
              "left-0"
            )} />
          )}
        </button>
        
        {/* Settings button removed */}
      </div>
    );
});

// Helper component for Slim Mode Buttons
const SlimButton = memo(({ icon: Icon, label, onClick, children }: any) => (
  <div className="relative group/slim-item w-full flex justify-center py-2">
     <button 
       onClick={onClick}
       className="w-10 h-10 flex items-center justify-center rounded-xl bg-gray-100 dark:bg-[#1E1F20] hover:bg-black dark:hover:bg-white hover:text-white dark:hover:text-black text-gray-500 dark:text-[#C4C7C5] transition-all active:scale-95 shadow-sm"
     >
        <Icon className="w-5 h-5" />
     </button>
     
     {/* Flyout Menu */}
     {children && (
       <div className="absolute left-full top-0 pl-4 hidden group-hover/slim-item:block z-50">
          <div className="w-64 bg-white dark:bg-[#1E1F20] border border-gray-100 dark:border-[#444746] rounded-2xl shadow-2xl p-2 animate-fadeIn origin-top-left max-h-[80vh] overflow-y-auto custom-scrollbar">
              <div className="px-3 py-2 text-xs font-black text-gray-400 dark:text-[#C4C7C5] uppercase tracking-widest border-b border-gray-50 dark:border-[#444746] mb-1">
                  {label}
              </div>
              {children}
          </div>
       </div>
     )}
  </div>
));

// Row component for react-window
const Row = memo(({ index, style, items, handlers, state }: any) => {
  const item = items[index];

  if (item.type === 'header_groups') {
    return (
      <div style={style} className="px-4 mb-2 flex justify-between items-center group/header">
         <div 
            onClick={handlers.toggleProjectsCollapsed}
            className="text-sm font-bold text-gray-500 dark:text-[#E3E3E3] uppercase tracking-wider flex items-center gap-2 hover:text-black dark:hover:text-white transition-colors cursor-pointer"
         >
            <Icons.ChevronRight className={clsx("w-3.5 h-3.5 transition-transform", state.isProjectsCollapsed ? "rotate-0" : "rotate-90")} />
            <span>{state.t.groups}</span> 
         </div>
         <button 
            onClick={(e) => { e.stopPropagation(); handlers.createGroup(); }} 
            className="hover:text-black dark:hover:text-white transition-all p-1 hover:bg-gray-200 dark:hover:bg-[#333537] rounded-md opacity-60 hover:opacity-100"
         >
             <Icons.Plus className="w-3.5 h-3.5" />
         </button>
      </div>
    );
  }

  if (item.type === 'header_recent') {
    return (
      <div style={style} className="px-4 mb-2 flex items-center gap-2 group/header">
         <div 
            onClick={handlers.toggleRecentCollapsed}
            className="text-sm font-bold text-gray-500 dark:text-[#E3E3E3] uppercase tracking-wider flex items-center gap-2 hover:text-black dark:hover:text-white transition-colors cursor-pointer"
         >
            <Icons.ChevronRight className={clsx("w-3.5 h-3.5 transition-transform", state.isRecentCollapsed ? "rotate-0" : "rotate-90")} />
            <span>{state.t.recent}</span>
         </div>
      </div>
    );
  }

  if (item.type === 'group') {
    const group = item.data;
    return (
      <div style={style} className="relative">
        <div className="absolute left-[23px] top-0 bottom-0 w-[1.5px] bg-gray-200 dark:bg-[#444746]" />
        <div style={{ paddingLeft: 36 }}>
            <NavItem 
              isSubItem={true}
              group={group}
              isEditing={state.editingId === group.id}
              editValue={state.editValue}
              onEditChange={handlers.setEditValue}
              onEditSubmit={handlers.handleRenameSubmit}
              onContextMenu={(e: any) => handlers.onContextMenuGroup(e, group)}
              onActionClick={(e: any) => handlers.onContextMenuGroup(e, group)}
              onClick={() => handlers.toggleGroupCollapse(group.id, group.collapsed)}
              onDrop={(e) => handlers.handleDropOnGroup(e, group.id)}
            >
              {group.title || state.t.untitledGroup}
            </NavItem>
        </div>
      </div>
    );
  }

  if (item.type === 'session' || item.type === 'session_recent') {
    const s = item.data;
    const isGroupSession = item.type === 'session';
    const indent = isGroupSession ? 54 : 36;
    
    return (
      <div style={style} className="relative">
         <div className="absolute left-[23px] top-0 bottom-0 w-[1.5px] bg-gray-200 dark:bg-[#444746]" />
         {/* Optional: Second line for group sessions? For now, just one main tree line */}
        <div style={{ paddingLeft: indent }}>
          <NavItem 
            isSubItem={true} 
            isActive={s.id === state.currentSessionId} 
            isEditing={state.editingId === s.id}
            editValue={state.editValue}
            onEditChange={handlers.setEditValue}
            onEditSubmit={handlers.handleRenameSubmit}
            onClick={() => handlers.selectSession(s.id)} 
            onContextMenu={(e: any) => handlers.onContextMenuSession(e, s)}
            onActionClick={(e: any) => handlers.onContextMenuSession(e, s)}
            draggable={true}
            onDragStart={(e) => handlers.handleDragStartSession(e, s.id)}
          >
             {s.title || "Untitled Chat"}
          </NavItem>
        </div>
      </div>
    );
  }

  return null;
}, (prevProps: any, nextProps: any) => {
  return (
    prevProps.index === nextProps.index &&
    prevProps.style === nextProps.style &&
    prevProps.items === nextProps.items &&
    prevProps.handlers === nextProps.handlers &&
    prevProps.state === nextProps.state
  );
});

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

  // Memoize filtered lists
  const activeGroups = useMemo(() => store.groups.filter(g => !g.isDeleted), [store.groups]);
  const activeSessions = useMemo(() => store.sessions.filter(s => !s.isDeleted), [store.sessions]);

  const groupedSessions = useMemo(() => activeGroups.map(g => ({ 
    group: g, 
    sessions: activeSessions.filter(s => s.groupId === g.id) 
  })), [activeGroups, activeSessions]);

  const ungrouped = useMemo(() => activeSessions.filter(s => !s.groupId), [activeSessions]);

  // 练习：API 调用示例 (不会执行)
  // Practice: API Call Example (Will not execute)
  /*
  useEffect(() => {
    if (store.currentSessionId) {
      getUserSessionHistory(store.currentSessionId)
        .then(history => console.log('Session history:', history))
        .catch(err => console.error('Failed to load history:', err));
    }
  }, [store.currentSessionId]);
  */

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
  }, [isResizing, store.setSidebarWidth]);

  const handleRenameSubmit = useCallback((cancel = false) => {
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
  }, [editingId, editValue, store.groups, store.updateGroup, store.renameSession]);

  const startRename = useCallback((id: string, initialValue: string) => {
    setEditingId(id);
    setEditValue(initialValue);
  }, []);

  const onContextMenuGroup = useCallback((e: any, g: Group) => {
    e.preventDefault();
    setContextMenu({ x: e.clientX, y: e.clientY, actions: [
      { label: t.rename, icon: Icons.Edit, onClick: () => startRename(g.id, g.title) },
      { label: t.settings, icon: Icons.Settings, onClick: () => { store.setEditingProject(g.id); store.setActiveModal('project_edit'); } },
      { separator: true },
      { label: t.delete, icon: Icons.Trash, onClick: () => store.deleteGroup(g.id), danger: true }
    ]});
  }, [t, startRename, store.setEditingProject, store.setActiveModal, store.deleteGroup]);

  const onContextMenuSession = useCallback((e: any, s: ChatSession) => {
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
  }, [t, activeGroups, startRename, store.moveSession, store.user?.email, store.deleteSession]);

  const handleDragStartSession = useCallback((e: React.DragEvent, sessionId: string) => {
      e.dataTransfer.setData('application/json', JSON.stringify({ type: 'session', id: sessionId }));
      e.dataTransfer.effectAllowed = 'move';
  }, []);

  const handleDropOnGroup = useCallback((e: React.DragEvent, groupId: string) => {
      try {
          const data = JSON.parse(e.dataTransfer.getData('application/json'));
          if (data.type === 'session' && data.id) {
              store.moveSession(data.id, groupId);
              store.updateGroup(groupId, { collapsed: false });
          }
      } catch (err) {
          console.error('Failed to parse drag data', err);
      }
  }, [store.moveSession, store.updateGroup]);

  const handleSlimProjectClick = useCallback((e: React.MouseEvent, group: Group) => {
    e.stopPropagation();
    const firstSession = activeSessions.find(s => s.groupId === group.id);
    if (firstSession) {
        store.selectSession(firstSession.id);
    }
  }, [activeSessions, store.selectSession]);

  const toggleGroupCollapse = useCallback((id: string, collapsed: boolean) => {
    store.updateGroup(id, { collapsed: !collapsed });
  }, [store.updateGroup]);

  const createGroup = useCallback(() => {
    store.createGroup(uuidv4(), t.untitledGroup);
  }, [store.createGroup, t.untitledGroup]);

  // Flatten data for react-window
  const flattenedItems = useMemo(() => {
    const items: any[] = [];
    
    if (!isSlim) {
      items.push({ type: 'header_groups' });
      
      if (!isProjectsCollapsed) {
        groupedSessions.forEach(({ group, sessions }) => {
          items.push({ type: 'group', data: group });
          if (!group.collapsed) {
            sessions.forEach(s => items.push({ type: 'session', data: s, groupId: group.id }));
          }
        });
      }
      
      items.push({ type: 'header_recent' });
      
      if (!isRecentCollapsed) {
        ungrouped.forEach(s => items.push({ type: 'session_recent', data: s }));
      }
    }
    
    return items;
  }, [isSlim, isProjectsCollapsed, isRecentCollapsed, groupedSessions, ungrouped]);

  const getItemSize = useCallback((index: number) => {
    const item = flattenedItems[index];
    if (item.type === 'header_groups' || item.type === 'header_recent') return 32;
    if (item.type === 'group') return 42;
    return 42; // session
  }, [flattenedItems]);

  const itemData = useMemo(() => ({
    items: flattenedItems,
    handlers: {
      toggleGroupCollapse,
      selectSession: store.selectSession,
      onContextMenuGroup,
      onContextMenuSession,
      handleDragStartSession,
      handleDropOnGroup,
      startRename,
      handleRenameSubmit,
      setEditValue,
      toggleProjectsCollapsed: () => setIsProjectsCollapsed(prev => !prev),
      toggleRecentCollapsed: () => setIsRecentCollapsed(prev => !prev),
      createGroup
    },
    state: {
      editingId,
      editValue,
      currentSessionId: store.currentSessionId,
      isProjectsCollapsed,
      isRecentCollapsed,
      t
    }
  }), [flattenedItems, toggleGroupCollapse, store.selectSession, onContextMenuGroup, onContextMenuSession, handleDragStartSession, handleDropOnGroup, startRename, handleRenameSubmit, editingId, editValue, store.currentSessionId, isProjectsCollapsed, isRecentCollapsed, t, createGroup]);

  return (
    <Profiler id="Sidebar" onRender={onRenderCallback}>
      <motion.aside 
        layout
        animate={{ width: isSlim ? 72 : sidebarWidth }}
        transition={{ duration: isResizing ? 0 : 0.35, ease: [0.4, 0, 0.2, 1] }}
        className="flex flex-col bg-[#F9FAFB] dark:bg-[#1E1F20] border-r border-gray-200 dark:border-[#444746] h-full select-none z-10 relative shrink-0"
      >
        {/* Header */}
        <div className="h-16 flex items-center justify-between px-5 shrink-0 overflow-hidden">
          {!isSlim && (
            <motion.div 
              initial={{ opacity: 0, x: -10 }}
              animate={{ opacity: 1, x: 0 }}
              className="flex items-center gap-3 flex-1 overflow-hidden"
            >
              <div className="w-8 h-8 flex items-center justify-center shrink-0">
                <OpenClaw.Color className="w-7 h-7" />
              </div>
              <span className="font-black text-xl tracking-tighter text-black dark:text-white uppercase truncate">Agent</span>
            </motion.div>
          )}
          
          <button 
              onClick={(e) => { e.stopPropagation(); store.toggleSidebar(); }}
              className={clsx(
                "w-10 h-10 bg-black dark:bg-[#28292A] text-white dark:text-[#E3E3E3] rounded-xl flex items-center justify-center shadow-lg hover:bg-gray-800 dark:hover:bg-[#333537] transition-all hover:scale-105 active:scale-95 shrink-0 border border-transparent dark:border-[#444746]",
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

        {/* Actions Section (New Chat & Search) */}
        <div className={clsx("mb-2 transition-all shrink-0 flex flex-col gap-1", isSlim ? "px-2" : "px-3")}>
           {/* New Chat */}
           <button 
             onClick={store.createNewSession} 
             className={clsx(
               "flex items-center rounded-xl transition-all duration-200 hover:bg-gray-200/50 dark:hover:bg-[#333537] text-gray-600 dark:text-[#C4C7C5] hover:text-black dark:hover:text-white",
               isSlim ? "w-10 h-10 justify-center mx-auto" : "w-full h-10 px-3 gap-3"
             )}
             title={t.newChat}
           >
             <Icons.NewChat className="w-5 h-5" />
             {!isSlim && <span className="text-sm font-bold">{t.newChat}</span>}
           </button>

           {/* Search */}
           <button 
              onClick={store.toggleSearch}
              className={clsx(
                  "flex items-center rounded-xl transition-all duration-200 hover:bg-gray-200/50 dark:hover:bg-[#333537] text-gray-600 dark:text-[#C4C7C5] hover:text-black dark:hover:text-white",
                   isSlim ? "w-10 h-10 justify-center mx-auto" : "w-full h-10 px-3 gap-3"
              )}
              title={t.search}
           >
               <Icons.Search className="w-5 h-5" />
               {!isSlim && <span className="text-sm font-bold">{t.search}</span>}
           </button>
        </div>

        {/* Navigation Content */}
        <div className={clsx("flex-1 relative", isSlim ? "overflow-visible" : "overflow-hidden")}>
          <AnimatePresence mode="popLayout">
            {!isSlim ? (
              <motion.div 
                key="full-rail"
                initial={{ opacity: 0 }} 
                animate={{ opacity: 1 }}
                exit={{ opacity: 0 }}
                className="h-full pt-2"
              >
                <AutoSizer>
                  {({ height, width }: any) => (
                    <List
                      style={{ width, height }}
                      rowCount={flattenedItems.length}
                      rowHeight={getItemSize}
                      rowComponent={Row as any}
                      rowProps={itemData}
                    />
                  )}
                </AutoSizer>
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
                                  onClick={(e) => handleSlimProjectClick(e, g)}
                                  className="w-full flex items-center gap-3 p-2 rounded-lg hover:bg-gray-50 dark:hover:bg-[#333537] transition-colors text-left group/item"
                                >
                                    <ProjectMarker group={g} />
                                    <span className="text-sm font-bold text-gray-700 dark:text-[#E3E3E3] truncate flex-1">{g.title || t.untitledGroup}</span>
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
                                      s.id === store.currentSessionId ? "bg-black dark:bg-[#004A77] text-white dark:text-[#E3E3E3]" : "text-gray-600 dark:text-[#C4C7C5] hover:bg-gray-100 dark:hover:bg-[#333537]"
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
              </motion.div>
            )}
          </AnimatePresence>
        </div>

        {/* User Section */}
        <div className="p-2 border-t border-gray-200 dark:border-[#444746] bg-[#F9FAFB] dark:bg-[#1E1F20] shrink-0 relative" ref={userMenuRef}>
           <AnimatePresence>
           {showUserMenu && (
             <motion.div 
               initial={{ opacity: 0, x: isSlim ? 10 : 0, y: 10, scale: 0.95 }} 
               animate={{ opacity: 1, x: 0, y: 0, scale: 1 }} 
               exit={{ opacity: 0, x: isSlim ? 10 : 0, y: 10, scale: 0.95 }}
               className={clsx(
                  "absolute bottom-full bg-white dark:bg-[#1E1F20] rounded-[22px] shadow-[0_15px_45px_-12px_rgba(0,0,0,0.15)] border border-gray-200 dark:border-[#444746] py-1.5 mb-3 z-[100] text-[13px] overflow-hidden whitespace-nowrap",
                  isSlim ? "left-14 w-52" : "left-2.5 right-2.5"
               )}
             >
                <div className="px-4 py-2 mb-1 border-b border-gray-50 dark:border-[#444746] bg-gray-50/50 dark:bg-[#1E1F20]/50">
                    <div className="font-black text-black dark:text-white text-xs truncate">{store.user?.name}</div>
                    <div className="text-[10px] text-gray-400 dark:text-gray-400 font-bold truncate mt-0.5">{store.user?.email}</div>
                </div>
                <button onClick={() => { setShowUserMenu(false); store.setActiveModal('account'); }} className="w-full px-4 py-2 text-left hover:bg-gray-50 dark:hover:bg-[#333537] flex items-center gap-2.5 font-bold text-gray-700 dark:text-gray-200 transition-colors">
                  <Icons.User className="w-3.5 h-3.5 text-gray-400 dark:text-gray-400" /> 
                  <span>{t.account}</span>
                </button>
                <button onClick={() => { setShowUserMenu(false); store.setActiveModal('settings'); }} className="w-full px-4 py-2 text-left hover:bg-gray-50 dark:hover:bg-[#333537] flex items-center gap-2.5 font-bold text-gray-700 dark:text-gray-200 transition-colors">
                  <Icons.Settings className="w-3.5 h-3.5 text-gray-400 dark:text-gray-400" /> 
                  <span>{t.settings}</span>
                </button>
                <button onClick={() => { setShowUserMenu(false); store.setActiveModal('help'); }} className="w-full px-4 py-2 text-left hover:bg-gray-50 dark:hover:bg-[#333537] flex items-center gap-2.5 font-bold text-gray-700 dark:text-gray-200 transition-colors">
                  <Icons.Help className="w-3.5 h-3.5 text-gray-400 dark:text-gray-400" /> 
                  <span>{t.getHelp}</span>
                </button>
                <div className="h-px bg-gray-100 dark:bg-[#444746] my-1 mx-4" />
                <button onClick={() => { setShowUserMenu(false); store.setActiveModal('upgrade'); }} className="w-full px-4 py-2 text-left hover:bg-yellow-50 dark:hover:bg-yellow-900/20 flex items-center gap-2.5 font-bold text-yellow-700 dark:text-yellow-500 transition-colors">
                  <Icons.CreditCard className="w-3.5 h-3.5 text-yellow-500" /> 
                  <span>{t.upgradeSubscription}</span>
                </button>
                <div className="h-px bg-gray-100 dark:bg-[#444746] my-1 mx-4" />
                <button onClick={() => { setShowUserMenu(false); store.logout(); }} className="w-full px-4 py-2 text-left hover:bg-red-50 dark:hover:bg-red-900/20 text-red-600 dark:text-red-400 flex items-center gap-2.5 font-bold transition-colors">
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
               "flex items-center gap-2.5 p-1.5 rounded-xl transition-all hover:bg-gray-200/50 dark:hover:bg-[#333537] w-full active:scale-[0.98]",
               isSlim ? "justify-center" : ""
             )}
           >
              <div className="w-11 h-11 md:w-8 md:h-8 rounded-full bg-gradient-to-tr from-gray-200 to-gray-300 dark:from-gray-700 dark:to-gray-600 flex items-center justify-center text-gray-600 dark:text-[#C4C7C5] shadow-sm border-2 border-white dark:border-[#444746] shrink-0 relative overflow-hidden transition-all">
                {store.user?.avatar ? (
                    <img src={store.user.avatar} className="w-full h-full object-cover" />
                ) : (
                    <Icons.User className="w-4.5 h-4.5" />
                )}
              </div>
              {!isSlim && (
                <div className="flex-1 text-left overflow-hidden">
                  <div className="text-[12px] font-bold text-black dark:text-[#E3E3E3] truncate leading-tight">{store.user?.name || "Agent 用户"}</div>
                  <div className="text-[9px] text-gray-400 dark:text-[#C4C7C5] font-bold truncate leading-none mt-0.5">{store.user?.email}</div>
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
    </Profiler>
  );
};
