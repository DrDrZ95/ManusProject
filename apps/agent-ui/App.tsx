
import React, { useEffect, useRef, useState } from 'react';
import { v4 as uuidv4 } from 'uuid';
import { useStore } from './store';
import { Sidebar } from './components/Sidebar';
import { InputArea } from './components/InputArea';
import { MessageBubble } from './components/MessageBubble';
import { MySpacePanel } from './components/MySpacePanel';
import { NewsGrid } from './components/NewsGrid';
import { UserModals } from './components/UserModals';
import { LoginPage } from './components/LoginPage';
import { Icons } from './components/icons';
import { aiService } from './services/ai';
import { Role, Attachment } from './types';
import { socketService } from './services/socket';
import clsx from 'clsx';
import { AnimatePresence, motion } from 'framer-motion';
import { translations } from './locales';

const Toast: React.FC = () => {
    const toast = useStore(s => s.toast);
    const setToast = useStore(s => s.setToast);

    useEffect(() => {
        if (toast) {
            const timer = setTimeout(() => setToast(null), 4000);
            return () => clearTimeout(timer);
        }
    }, [toast, setToast]);

    return (
        <AnimatePresence>
            {toast && (
                <motion.div 
                    initial={{ y: 50, opacity: 0 }}
                    animate={{ y: 0, opacity: 1 }}
                    exit={{ y: 50, opacity: 0 }}
                    className="fixed bottom-24 right-6 z-[100] flex items-center gap-3 bg-black text-white px-5 py-3 rounded-2xl shadow-2xl border border-white/10"
                >
                    <div className={clsx(
                        "p-1.5 rounded-full",
                        toast.type === 'info' ? "bg-blue-500" : toast.type === 'success' ? "bg-green-500" : "bg-amber-500"
                    )}>
                        <Icons.Check className="w-3.5 h-3.5" />
                    </div>
                    <span className="text-xs font-black tracking-tight">{toast.message}</span>
                    <button onClick={() => setToast(null)} className="ml-4 p-1 hover:bg-white/10 rounded-full transition-colors">
                        <Icons.Close className="w-3.5 h-3.5" />
                    </button>
                </motion.div>
            )}
        </AnimatePresence>
    );
}

const App: React.FC = () => {
  const isAuthenticated = useStore(s => s.isAuthenticated);
  const sessions = useStore(s => s.sessions);
  const currentSessionId = useStore(s => s.currentSessionId);
  const input = useStore(s => s.input);
  const attachments = useStore(s => s.attachments);
  const selectedModel = useStore(s => s.selectedModel);
  const isLoading = useStore(s => s.isLoading);
  const isSidebarOpen = useStore(s => s.isSidebarOpen);
  const isTerminalOpen = useStore(s => s.isTerminalOpen);
  const terminalWidth = useStore(s => s.terminalWidth);
  const language = useStore(s => s.language);
  const news = useStore(s => s.news);
  const inputMode = useStore(s => s.inputMode); 
  
  const setInput = useStore(s => s.setInput);
  const addMessage = useStore(s => s.addMessage);
  const updateLastMessage = useStore(s => s.updateLastMessage);
  const setLoading = useStore(s => s.setLoading);
  const toggleSidebar = useStore(s => s.toggleSidebar);
  const toggleTerminal = useStore(s => s.toggleTerminal);
  const setTerminalWidth = useStore(s => s.setTerminalWidth);
  const fetchNews = useStore(s => s.fetchNews);
  const clearAttachments = useStore(s => s.clearAttachments);

  const chatEndRef = useRef<HTMLDivElement>(null);
  
  const [isDragging, setIsDragging] = useState(false);
  const dragStartX = useRef(0);
  const dragStartWidth = useRef(0);

  const currentSession = sessions.find(s => s.id === currentSessionId);
  const messages = currentSession?.messages || [];
  
  const t = translations[language];

  useEffect(() => {
    if (isAuthenticated) {
        fetchNews();
        socketService.connect();
        const interval = setInterval(() => fetchNews(), 6 * 60 * 60 * 1000);
        return () => {
          clearInterval(interval);
          socketService.disconnect();
        };
    }
  }, [isAuthenticated, fetchNews]);

  useEffect(() => {
    if (isAuthenticated) {
        chatEndRef.current?.scrollIntoView({ behavior: 'smooth' });
    }
  }, [messages.length, currentSessionId, isAuthenticated]);

  useEffect(() => {
    const handleMouseMove = (e: MouseEvent) => {
      if (isDragging) {
        const delta = dragStartX.current - e.clientX;
        const newWidth = Math.max(300, Math.min(800, dragStartWidth.current + delta));
        setTerminalWidth(newWidth);
      }
    };
    const handleMouseUp = () => setIsDragging(false);

    if (isDragging) {
      document.addEventListener('mousemove', handleMouseMove);
      document.addEventListener('mouseup', handleMouseUp);
      document.body.style.cursor = 'col-resize';
    } else {
      document.body.style.cursor = 'default';
    }

    return () => {
      document.removeEventListener('mousemove', handleMouseMove);
      document.removeEventListener('mouseup', handleMouseUp);
      document.body.style.cursor = 'default';
    };
  }, [isDragging, setTerminalWidth]);

  const handleSend = async () => {
    if (!currentSessionId || (!input.trim() && attachments.length === 0) || isLoading) return;

    const userMsgContent = input;
    const currentAttachments = [...attachments];
    
    setInput('');
    clearAttachments();
    setLoading(true);

    let displayContent = userMsgContent;
    if (currentAttachments.length > 0) {
        displayContent += `\n\n` + currentAttachments.map(a => `📎 [${a.name}]`).join('\n');
    }

    addMessage(currentSessionId, {
      id: uuidv4(),
      role: Role.USER,
      content: displayContent,
      timestamp: Date.now(),
      attachments: currentAttachments
    });

    const aiMsgId = uuidv4();
    addMessage(currentSessionId, {
      id: aiMsgId,
      role: Role.MODEL,
      content: '',
      timestamp: Date.now(),
      isStreaming: true
    });

    const history = messages.map(m => ({
        role: m.role === Role.USER ? 'user' : 'model',
        parts: [{ text: m.content }]
    }));

    await aiService.streamResponse(
      {
        model: selectedModel,
        history,
        prompt: userMsgContent,
        attachments: currentAttachments,
        inputMode
      },
      {
        onChunk: (text) => {
          updateLastMessage(currentSessionId, text);
        },
        onFinish: () => {
          setLoading(false);
          useStore.setState((state) => {
             const sessionIndex = state.sessions.findIndex(s => s.id === currentSessionId);
             if (sessionIndex === -1) return state;
             const updatedSessions = [...state.sessions];
             const msgs = [...updatedSessions[sessionIndex].messages];
             const lastMsg = msgs[msgs.length - 1];
             if (lastMsg.role === Role.MODEL) {
                 msgs[msgs.length - 1] = { ...lastMsg, isStreaming: false };
             }
             updatedSessions[sessionIndex] = { ...updatedSessions[sessionIndex], messages: msgs };
             return { sessions: updatedSessions };
          });
        },
        onError: (err) => {
          setLoading(false);
          updateLastMessage(currentSessionId, "Error: Could not connect to Agent services. Please try again.");
          useStore.setState((state) => {
            const sessionIndex = state.sessions.findIndex(s => s.id === currentSessionId);
            if (sessionIndex === -1) return state;
            const updatedSessions = [...state.sessions];
            const msgs = [...updatedSessions[sessionIndex].messages];
            const lastMsg = msgs[msgs.length - 1];
            if (lastMsg.role === Role.MODEL) {
                msgs[msgs.length - 1] = { ...lastMsg, isStreaming: false };
            }
            updatedSessions[sessionIndex] = { ...updatedSessions[sessionIndex], messages: msgs };
            return { sessions: updatedSessions };
         });
        }
      }
    );
  };

  if (!isAuthenticated) {
    return <LoginPage />;
  }

  return (
    <div className="flex h-screen w-full overflow-hidden bg-[#F3F4F6] text-gray-900 font-sans selection:bg-gray-300 selection:text-black">
      <Toast />
      <UserModals />
      
      <Sidebar />
      
      {/* MOBILE HEADER */}
       <div className="md:hidden fixed top-0 left-0 right-0 h-14 bg-[#F3F4F6]/80 backdrop-blur border-b border-gray-200 flex items-center justify-between px-4 z-40">
          <button onClick={toggleSidebar} className="text-gray-600">
             <Icons.Sidebar className="w-6 h-6" />
          </button>
          <div className="flex items-center gap-2">
            <Icons.Zap className="w-5 h-5 text-black fill-black" />
            <span className="font-bold text-lg tracking-tight text-black">Agent</span>
          </div>
          <button onClick={toggleTerminal} className={clsx("text-gray-600", isTerminalOpen && "text-black")}>
             <Icons.Folder className="w-5 h-5" />
          </button>
       </div>

      <main className="flex-1 flex flex-col min-w-0 relative h-full z-0">
        <div className="flex-1 overflow-y-auto scrollbar-hide pt-20 md:pt-4">
           {messages.length === 0 ? (
             <div className="h-full flex flex-col items-center justify-center opacity-0 animate-[fadeIn_0.5s_ease-out_forwards] pb-20 px-4">
                <div className="flex flex-col items-center text-center max-w-2xl w-full">
                    <div className="w-16 h-16 mb-6 rounded-2xl bg-white text-black flex items-center justify-center shadow-lg border border-gray-100">
                      <Icons.Zap className="w-8 h-8" fill="currentColor" />
                    </div>
                    <h1 className="text-2xl font-semibold mb-2 text-gray-900">{t.grokIntroTitle}</h1>
                    <p className="text-gray-500 mb-8 px-4">
                      {t.grokIntroDesc}
                    </p>
                    <NewsGrid news={news} />
                </div>
             </div>
           ) : (
             <div className="w-full max-w-[95%] lg:max-w-5xl mx-auto px-4 pb-4">
               {messages.map((msg, idx) => (
                 <MessageBubble 
                    key={msg.id} 
                    message={msg} 
                    isLast={idx === messages.length - 1} 
                 />
               ))}
               <div ref={chatEndRef} className="h-4" />
             </div>
           )}
        </div>

        <div className="w-full z-20 bg-gradient-to-t from-[#F3F4F6] via-[#F3F4F6] to-transparent pt-10 pb-6 px-4">
           <InputArea onSend={handleSend} />
        </div>
      </main>

      <AnimatePresence>
      {isTerminalOpen && (
        <motion.div
          initial={{ width: 0, opacity: 0 }}
          animate={{ width: 'auto', opacity: 1 }}
          exit={{ width: 0, opacity: 0 }}
          transition={{ duration: 0.3, ease: [0.4, 0, 0.2, 1] }}
          className="flex h-full shrink-0"
        >
           <div 
              className="hidden md:block w-1 hover:w-1.5 bg-transparent hover:bg-gray-300 transition-all cursor-col-resize z-40 h-full relative -mr-0.5"
              onMouseDown={(e) => {
                setIsDragging(true);
                dragStartX.current = e.clientX;
                dragStartWidth.current = terminalWidth;
              }}
           />
           
           <div 
             style={{ width: isDragging ? terminalWidth : `${terminalWidth}px` }}
             className="w-full md:w-auto shrink-0 h-full shadow-[-10px_0_30px_rgba(0,0,0,0.05)] z-30 absolute md:relative right-0 bg-white overflow-hidden"
           >
              <MySpacePanel />
           </div>
        </motion.div>
      )}
      </AnimatePresence>

      <AnimatePresence>
      {!isTerminalOpen && (
         <motion.div 
           initial={{ opacity: 0, y: 10 }}
           animate={{ opacity: 1, y: 0 }}
           exit={{ opacity: 0, y: 10 }}
           className="hidden lg:block absolute top-4 right-4 z-30"
         >
             <button 
               onClick={toggleTerminal}
               className="flex items-center gap-2 px-3 py-2 bg-white border border-gray-200 rounded-lg text-sm text-gray-500 hover:text-black hover:border-gray-400 transition-all shadow-sm"
             >
               <Icons.Folder className="w-4 h-4" />
               <span>{t.mySpace}</span>
             </button>
         </motion.div>
      )}
      </AnimatePresence>
    </div>
  );
};

export default App;
