
import React, { useRef, useEffect } from 'react';
import { useStore } from '../store';
import { Icons } from './icons';
import { ModelSelector } from './ModelSelector';
import { InputModeSelector } from './InputModeSelector';
import { translations } from '../locales';
import clsx from 'clsx';

interface InputAreaProps {
  onSend: () => void;
}

export const InputArea: React.FC<InputAreaProps> = ({ onSend }) => {
  const input = useStore(s => s.input);
  const setInput = useStore(s => s.setInput);
  const isLoading = useStore(s => s.isLoading);
  const language = useStore(s => s.language);
  const attachments = useStore(s => s.attachments);
  const addAttachment = useStore(s => s.addAttachment);
  const removeAttachment = useStore(s => s.removeAttachment);
  const inputMode = useStore(s => s.inputMode);
  
  const textareaRef = useRef<HTMLTextAreaElement>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);
  
  const t = translations[language];

  // Determine placeholder based on mode
  const getPlaceholder = () => {
    switch (inputMode) {
        case 'work_report': return t.placeholderWorkReport;
        case 'oa_workflow': return t.placeholderOA;
        case 'project': return t.placeholderProject;
        case 'company': return t.placeholderCompany;
        default: return t.placeholderGeneral;
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

  return (
    <div className="w-full max-w-[95%] lg:max-w-5xl mx-auto">
      <div className={clsx(
        "relative group bg-white rounded-[26px] border transition-all duration-300 ease-out",
        "focus-within:border-gray-400 focus-within:shadow-lg shadow-sm",
        "border-gray-200"
      )}>
        
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

        {/* Text Area */}
        <textarea
          ref={textareaRef}
          value={input}
          onChange={(e) => setInput(e.target.value)}
          onKeyDown={handleKeyDown}
          placeholder={getPlaceholder()}
          rows={1}
          className="w-full bg-transparent text-gray-900 placeholder-gray-400 px-4 py-4 text-[16px] resize-none outline-none max-h-[200px] overflow-y-auto scrollbar-hide"
          style={{ minHeight: '56px' }}
        />

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
      </div>
      <div className="text-center mt-2 text-xs text-gray-400 font-medium">
        {t.disclaimer}
      </div>
    </div>
  );
};
