
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
  
  const [isVoiceMode, setIsVoiceMode] = useState(false);
  const [isRecording, setIsRecording] = useState(false);
  
  const textareaRef = useRef<HTMLTextAreaElement>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);
  
  const t = translations[language];

  // Determine placeholder based on mode
  const getPlaceholder = () => {
    switch (inputMode) {
        case 'brainstorm': return t.placeholderBrainstorm;
        case 'oa_work': return t.placeholderOAWork;
        case 'company': return t.placeholderCompany;
        default: return t.placeholderGeneral;
    }
  };

  // Unified Simplified Slash Commands (Language Agnostic)
  const getModeLabel = () => {
     switch (inputMode) {
        case 'brainstorm': return `/Brainstorm`;
        case 'oa_work': return `/OA_Work`;
        case 'company': return `/Company`;
        default: return '';
     }
  };

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
    if (e.key === 'Backspace' && input === '' && inputMode !== 'general') {
        setInputMode('general');
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
    }, 800); // 800ms simulated recording time
  };

  return (
    <div className="w-full max-w-[95%] lg:max-w-5xl mx-auto">
      <div className={clsx(
        "relative group bg-white rounded-[26px] border transition-all duration-300 ease-out",
        "focus-within:border-gray-400 focus-within:shadow-lg shadow-sm",
        "border-gray-200"
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

              {/* Main Input Container - Flex Row Layout */}
              <div className="flex w-full items-start px-4 py-4 gap-2">
                  {/* Mode Chip Block (Inline) */}
                  {inputMode !== 'general' && (
                     <motion.div 
                        initial={{ scale: 0.9, opacity: 0 }}
                        animate={{ scale: 1, opacity: 1 }}
                        className="flex-shrink-0 pt-0.5"
                     >
                        <span className="inline-flex items-center gap-1.5 bg-blue-50 text-blue-600 px-2.5 py-1 rounded-md text-sm font-mono font-medium select-none border border-blue-100 cursor-default">
                            {getModeLabel()}
                            <button 
                                onClick={() => setInputMode('general')}
                                className="text-blue-400 hover:text-blue-700 rounded-full transition-colors flex items-center justify-center"
                                title="Clear mode"
                            >
                                <Icons.Close className="w-3 h-3" />
                            </button>
                        </span>
                     </motion.div>
                  )}

                  {/* Text Area */}
                  <textarea
                    ref={textareaRef}
                    value={input}
                    onChange={(e) => setInput(e.target.value)}
                    onKeyDown={handleKeyDown}
                    placeholder={getPlaceholder()}
                    rows={1}
                    className="flex-1 min-w-0 bg-transparent text-gray-900 placeholder-gray-400 text-[16px] resize-none outline-none overflow-y-auto scrollbar-hide py-1 leading-relaxed"
                    style={{ minHeight: '32px', maxHeight: '200px' }}
                  />
              </div>

              {/* Actions Bar */}
              <div className="flex items-center justify-between px-2 pb-2">
                <div className="flex items-center gap-2">
                  <ModelSelector />
                  
                  <div className="w-px h-4 bg-gray-200 mx-1" />
                  
                  <InputModeSelector />

                  <input 
                    type="file" 
                    ref={fileInputRef}
                    className="hidden"
                    accept=".pdf,.doc,.docx,.md,.txt"
                    onChange={handleFileSelect}
                  />
                  <button 
                    onClick={() => fileInputRef.current?.click()}
                    className="p-2 text-gray-400 hover:text-black transition-colors rounded-full hover:bg-gray-100"
                    title="Attach file (PDF, Word, MD)"
                  >
                    <Icons.Attach className="w-5 h-5" />
                  </button>
                </div>

                <div className="flex items-center gap-2">
                   {/* Voice Input Button */}
                   <button
                    onClick={() => setIsVoiceMode(true)}
                    className="p-2 text-gray-400 hover:text-black transition-colors rounded-full hover:bg-gray-100"
                    title="Voice Input"
                   >
                     <Icons.Mic className="w-5 h-5" />
                   </button>

                   {/* Send Button */}
                   <button
                    onClick={onSend}
                    disabled={(!input.trim() && attachments.length === 0) && !isLoading}
                    className={clsx(
                      "flex items-center justify-center w-8 h-8 rounded-full transition-all duration-200 shadow-sm",
                      (input.trim() || attachments.length > 0) || isLoading 
                        ? "bg-black text-white hover:bg-gray-800 hover:scale-105" 
                        : "bg-gray-200 text-gray-400 cursor-not-allowed"
                    )}
                   >
                    {isLoading ? (
                      <div className="w-3 h-3 bg-white rounded-sm animate-spin" />
                    ) : (
                      <Icons.Send className="w-4 h-4 ml-0.5" />
                    )}
                   </button>
                </div>
              </div>
            </motion.div>
          ) : (
            <motion.div
              key="voice-mode"
              initial={{ opacity: 0, height: 56 }}
              animate={{ opacity: 1, height: 200 }}
              exit={{ opacity: 0, height: 56 }}
              className="flex flex-col items-center justify-center h-[200px] relative bg-gradient-to-b from-white to-gray-50 rounded-[25px] overflow-hidden"
            >
               <button 
                 onClick={() => setIsVoiceMode(false)}
                 className="absolute top-3 right-3 p-2 text-gray-400 hover:text-black hover:bg-gray-200 rounded-full transition-colors z-20"
               >
                 <Icons.Close className="w-5 h-5" />
               </button>

               <div className="relative">
                 {/* Pulsing Rings */}
                 <motion.div 
                    animate={{ scale: [1, 1.5, 1], opacity: [0.5, 0, 0.5] }}
                    transition={{ repeat: Infinity, duration: 2 }}
                    className="absolute inset-0 bg-red-100 rounded-full z-0"
                 />
                 {isRecording && (
                   <motion.div 
                      animate={{ scale: [1.5, 2, 1.5], opacity: [0.3, 0, 0.3] }}
                      transition={{ repeat: Infinity, duration: 2, delay: 0.5 }}
                      className="absolute inset-0 bg-red-100 rounded-full z-0"
                   />
                 )}
                 
                 <motion.button
                   whileHover={{ scale: 1.05 }}
                   whileTap={{ scale: 0.95 }}
                   onMouseDown={handleVoiceInteraction}
                   onClick={handleVoiceInteraction}
                   className="relative z-10 w-24 h-24 bg-red-500 text-white rounded-full flex items-center justify-center shadow-xl hover:bg-red-600 transition-colors"
                 >
                   <Icons.Mic className="w-10 h-10" />
                 </motion.button>
               </div>
               
               <p className="mt-6 text-gray-500 font-medium text-sm z-10">
                 {isRecording ? t.listening : t.tapToSpeak}
               </p>
            </motion.div>
          )}
        </AnimatePresence>
      </div>
      <div className="text-center mt-2 text-xs text-gray-400 font-medium">
        {t.disclaimer}
      </div>
    </div>
  );
};
