
import React, { useRef, useEffect, useState } from 'react';
import { useStore } from '../store';
import { Icons } from './icons';
import { ModelSelector } from './ModelSelector';
import { InputModeSelector } from './InputModeSelector';
import { translations } from '../locales';
import clsx from 'clsx';
import { motion, AnimatePresence } from 'framer-motion';

interface InputAreaProps {
  onSend: () => void;
}

const VOICE_SIMULATIONS = [
  "帮我写一份关于下周项目计划的周报草稿。",
  "解释一下 React 中的 useEffect 是如何工作的？",
  "查询一下明天杭州的天气怎么样。",
  "帮我生成一个 Python 的快速排序算法。",
  "分析一下目前 AI 行业的发展趋势。"
];

export const InputArea: React.FC<InputAreaProps> = ({ onSend }) => {
  const input = useStore(s => s.input);
  const setInput = useStore(s => s.setInput);
  const isLoading = useStore(s => s.isLoading);
  const language = useStore(s => s.language);
  const attachments = useStore(s => s.attachments);
  const addAttachment = useStore(s => s.addAttachment);
  const removeAttachment = useStore(s => s.removeAttachment);
  const inputMode = useStore(s => s.inputMode);
  const setInputMode = useStore(s => s.setInputMode);
  const isAgentMode = useStore(s => s.isAgentMode);
  const setAgentMode = useStore(s => s.setAgentMode);
  
  const [isVoiceMode, setIsVoiceMode] = useState(false);
  const [isRecording, setIsRecording] = useState(false);
  const [showAgentConfirm, setShowAgentConfirm] = useState(false);
  
  const textareaRef = useRef<HTMLTextAreaElement>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);
  
  const t = translations[language];

  // Determine placeholder based on mode
  const getPlaceholder = () => {
    if (isAgentMode) return t.placeholderAgent;
    switch (inputMode) {
        case 'brainstorm': return t.placeholderBrainstorm;
        case 'oa_work': return t.placeholderOAWork;
        case 'company': return t.placeholderCompany;
        default: return t.placeholderGeneral;
    }
  };

  // Unified Simplified Slash Commands (Language Agnostic)
  const getModeLabel = () => {
     if (isAgentMode) return "Agent";
     switch (inputMode) {
        case 'brainstorm': return "Brainstorm";
        case 'oa_work': return "OA_Work";
        case 'company': return "Company";
        default: return null;
     }
  };

  const modeLabel = getModeLabel();

  // Auto-resize textarea
  useEffect(() => {
    if (textareaRef.current) {
      textareaRef.current.style.height = 'auto';
      textareaRef.current.style.height = Math.min(textareaRef.current.scrollHeight, 200) + 'px';
    }
  }, [input]);

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      if ((input.trim() || attachments.length > 0) && !isLoading) {
        onSend();
      }
    }
    // Handle deleting the mode block on backspace if input is empty
    if (e.key === 'Backspace' && input === '' && (inputMode !== 'general' || isAgentMode)) {
        setInputMode('general');
        setAgentMode(false); 
    }
  };

  const handleFileSelect = async (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      const file = e.target.files[0];
      const ext = file.name.split('.').pop()?.toLowerCase();
      const allowed = ['pdf', 'doc', 'docx', 'md', 'txt'];
      
      if (!ext || !allowed.includes(ext)) {
        alert('Only PDF, Word, and Markdown files are allowed.');
        return;
      }

      await addAttachment(file);
      if (fileInputRef.current) fileInputRef.current.value = '';
    }
  };

  const handleVoiceInteraction = () => {
    setIsRecording(true);
    
    // Simulate a short recording delay then finish
    setTimeout(() => {
      setIsRecording(false);
      setIsVoiceMode(false);
      
      // Select random simulation text
      const randomText = VOICE_SIMULATIONS[Math.floor(Math.random() * VOICE_SIMULATIONS.length)];
      setInput(randomText);
    }, 1500); // Slightly longer simulation
  };

  const handleAgentModeSelect = () => {
      setShowAgentConfirm(true);
  };

  const confirmAgentMode = () => {
      setAgentMode(true);
      setInputMode('agent'); // Force update internal input mode
      setShowAgentConfirm(false);
  };

  return (
    <>
    <div className="w-full max-w-[95%] lg:max-w-5xl mx-auto relative">
      <div className={clsx(
        "relative group bg-white rounded-[26px] border transition-all duration-300 ease-out",
        isAgentMode 
            ? "border-blue-500 shadow-[0_0_20px_rgba(59,130,246,0.2)]" 
            : "border-gray-200 focus-within:border-gray-400 focus-within:shadow-lg shadow-sm"
      )}>
        
        <AnimatePresence mode="wait">
          {!isVoiceMode ? (
            <motion.div
              key="text-mode"
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
            >
              {/* Attachments Preview */}
              {attachments.length > 0 && (
                <div className="px-4 pt-4 flex flex-wrap gap-2">
                  {attachments.map(att => (
                    <div key={att.id} className="flex items-center gap-2 bg-gray-100 px-3 py-1.5 rounded-lg text-xs font-medium text-gray-700 border border-gray-200">
                      <Icons.Attach className="w-3 h-3 text-gray-500" />
                      <span className="max-w-[150px] truncate">{att.name}</span>
                      <button onClick={() => removeAttachment(att.id)} className="hover:text-red-500 ml-1">
                        <Icons.Close className="w-3 h-3" />
                      </button>
                    </div>
                  ))}
                </div>
              )}

              {/* Main Input Container */}
              <div className="flex w-full items-end px-4 py-4 gap-2">
                  {/* Mode Chip Block (Inline) */}
                  {modeLabel && (
                     <motion.div 
                        initial={{ scale: 0.9, opacity: 0 }}
                        animate={{ scale: 1, opacity: 1 }}
                        className={clsx(
                          "flex-shrink-0 mb-1",
                          isAgentMode ? "text-blue-600" : "text-gray-600"
                        )}
                     >
                        <div className={clsx(
                          "flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-bold uppercase tracking-wide border select-none",
                          isAgentMode 
                            ? "bg-blue-50 border-blue-200" 
                            : "bg-gray-100 border-gray-200"
                        )}>
                            {isAgentMode ? <Icons.Cpu className="w-3.5 h-3.5" /> : <Icons.Zap className="w-3.5 h-3.5" />}
                            <span>{modeLabel}</span>
                            <button 
                              onClick={() => {
                                setInputMode('general');
                                setAgentMode(false);
                              }}
                              className={clsx("ml-1 hover:text-black", isAgentMode ? "hover:bg-blue-100" : "hover:bg-gray-200", "rounded-full p-0.5")}
                            >
                                <Icons.Close className="w-3 h-3" />
                            </button>
                        </div>
                     </motion.div>
                  )}

                  <textarea
                    ref={textareaRef}
                    value={input}
                    onChange={(e) => setInput(e.target.value)}
                    onKeyDown={handleKeyDown}
                    placeholder={getPlaceholder()}
                    rows={1}
                    className="flex-1 max-h-[200px] resize-none bg-transparent outline-none text-gray-900 placeholder-gray-400 leading-relaxed font-normal min-h-[24px] py-1"
                  />
                  
                  {/* Action Buttons: Voice and Send */}
                  <div className="flex items-center gap-2 mb-0.5">
                    <button 
                      onClick={() => setIsVoiceMode(true)}
                      className="p-2 text-gray-400 hover:text-black transition-colors rounded-full hover:bg-gray-100"
                      title="Voice Input"
                    >
                      <Icons.Mic className="w-6 h-6" />
                    </button>

                    <button
                      onClick={onSend}
                      disabled={(!input.trim() && attachments.length === 0) || isLoading}
                      className={clsx(
                        "w-9 h-9 flex items-center justify-center rounded-full transition-all duration-200",
                        (input.trim() || attachments.length > 0) && !isLoading
                          ? isAgentMode ? "bg-blue-600 hover:bg-blue-700 text-white shadow-md" : "bg-black hover:bg-gray-800 text-white shadow-md"
                          : "bg-gray-100 text-gray-300 cursor-not-allowed"
                      )}
                    >
                      {isLoading ? (
                        <div className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                      ) : (
                        <Icons.Send className="w-4 h-4 ml-0.5" />
                      )}
                    </button>
                  </div>
              </div>

              {/* Toolbar */}
              <div className="flex items-center justify-between px-3 pb-3 pt-0">
                <div className="flex items-center gap-1">
                  {/* Model Selector First */}
                  <ModelSelector />

                  <div className="w-px h-4 bg-gray-200 mx-1" />

                  {/* Input Mode Selector Second */}
                  <InputModeSelector onSelectAgentMode={handleAgentModeSelect} />
                  
                  <div className="w-px h-4 bg-gray-200 mx-1" />

                  <button 
                    onClick={() => fileInputRef.current?.click()}
                    className="p-2 text-gray-400 hover:text-black transition-colors rounded-full hover:bg-gray-100"
                    title="Attach File"
                  >
                    <Icons.Attach className="w-5 h-5" />
                  </button>
                  <input 
                    type="file" 
                    ref={fileInputRef} 
                    className="hidden" 
                    onChange={handleFileSelect}
                    accept=".pdf,.doc,.docx,.md,.txt"
                  />
                </div>
                
                <div className="flex items-center gap-3 text-xs text-gray-400 pr-2">
                   {isAgentMode && (
                       <span className="flex items-center gap-1 text-blue-500 font-medium animate-pulse">
                           <Icons.Cpu className="w-3 h-3" />
                           Agent Active
                       </span>
                   )}
                </div>
              </div>
            </motion.div>
          ) : (
            /* Voice Mode UI */
            <motion.div
               key="voice-mode"
               initial={{ opacity: 0, scale: 0.95 }}
               animate={{ opacity: 1, scale: 1 }}
               exit={{ opacity: 0, scale: 0.95 }}
               className="h-[180px] flex flex-col items-center justify-center relative bg-gradient-to-b from-gray-50 to-white rounded-[26px]"
            >
               <button 
                  onClick={() => setIsVoiceMode(false)}
                  className="absolute top-4 right-4 p-2 text-gray-400 hover:text-black rounded-full hover:bg-gray-100"
               >
                  <Icons.Close className="w-5 h-5" />
               </button>

               <div className="text-center w-full flex flex-col items-center">
                  <div className="mb-4 relative">
                     <button 
                        onClick={handleVoiceInteraction}
                        className={clsx(
                           "w-20 h-20 rounded-full flex items-center justify-center transition-all duration-300 shadow-xl border-4",
                           isRecording 
                            ? "bg-red-500 border-red-100 scale-110 shadow-red-200" 
                            : "bg-black border-gray-100 hover:bg-gray-800 hover:scale-105"
                        )}
                     >
                        <Icons.Mic className="w-8 h-8 text-white" />
                     </button>
                     {isRecording && (
                        <div className="absolute inset-0 rounded-full border-4 border-red-200 animate-ping opacity-75"></div>
                     )}
                  </div>
                  <p className="text-sm font-medium text-gray-600 text-center">
                     {isRecording ? t.listening : t.tapToSpeak}
                  </p>
               </div>
            </motion.div>
          )}
        </AnimatePresence>
      </div>
      
      {/* Agent Mode Confirmation Modal */}
      <AnimatePresence>
        {showAgentConfirm && (
           <motion.div
             initial={{ opacity: 0 }}
             animate={{ opacity: 1 }}
             exit={{ opacity: 0 }}
             className="fixed inset-0 z-50 bg-black/30 backdrop-blur-sm flex items-center justify-center p-4"
             onClick={() => setShowAgentConfirm(false)}
           >
              <motion.div 
                 initial={{ scale: 0.9, y: 20 }}
                 animate={{ scale: 1, y: 0 }}
                 exit={{ scale: 0.9, y: 20 }}
                 className="bg-white rounded-2xl p-6 max-w-sm w-full shadow-2xl border border-gray-100"
                 onClick={e => e.stopPropagation()}
              >
                  <div className="w-12 h-12 bg-blue-100 text-blue-600 rounded-2xl flex items-center justify-center mb-4 mx-auto">
                      <Icons.Cpu className="w-6 h-6" />
                  </div>
                  <h3 className="text-xl font-bold text-center text-gray-900 mb-2">{t.agentModeConfirmTitle}</h3>
                  <p className="text-center text-gray-500 mb-6 text-sm">
                      {t.agentModeConfirmDesc}
                  </p>
                  <div className="flex gap-3">
                      <button 
                          onClick={() => setShowAgentConfirm(false)}
                          className="flex-1 py-2.5 bg-gray-100 text-gray-700 font-bold rounded-xl hover:bg-gray-200 transition-colors"
                      >
                          {t.cancel}
                      </button>
                      <button 
                          onClick={confirmAgentMode}
                          className="flex-1 py-2.5 bg-blue-600 text-white font-bold rounded-xl hover:bg-blue-700 transition-colors shadow-lg shadow-blue-200"
                      >
                          {t.confirm}
                      </button>
                  </div>
              </motion.div>
           </motion.div>
        )}
      </AnimatePresence>
    </div>
    </>
  );
};
