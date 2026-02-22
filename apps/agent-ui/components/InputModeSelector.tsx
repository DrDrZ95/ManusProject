
import React, { useState, useRef, useEffect } from 'react';
import { useStore } from '../store';
import { Icons } from './icons';
import { InputMode } from '../types';
import { translations } from '../locales';
import clsx from 'clsx';

interface InputModeSelectorProps {
    onSelectAgentMode?: () => void;
}

export const InputModeSelector: React.FC<InputModeSelectorProps> = ({ onSelectAgentMode }) => {
  const [isOpen, setIsOpen] = useState(false);
  const inputMode = useStore(s => s.inputMode);
  const setInputMode = useStore(s => s.setInputMode);
  const setAgentMode = useStore(s => s.setAgentMode);
  const language = useStore(s => s.language);
  const dropdownRef = useRef<HTMLDivElement>(null);
  
  const t = translations[language];

  const modes: { id: InputMode; label: string; icon: any }[] = [
    { id: 'general', label: t.modeGeneral, icon: Icons.Zap },
    { id: 'brainstorm', label: t.modeBrainstorm, icon: Icons.Lightbulb },
    { id: 'oa_work', label: t.modeOAWork, icon: Icons.Briefcase },
    { id: 'company', label: t.modeCompany, icon: Icons.Building2 },
    { id: 'agent', label: t.modeAgent, icon: Icons.Cpu },
  ];

  const activeMode = modes.find(m => m.id === inputMode) || modes[0];

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  return (
    <div className="relative" ref={dropdownRef}>
      <button 
        onClick={() => setIsOpen(!isOpen)}
        className="p-2 text-gray-400 dark:text-[#C4C7C5] hover:text-black dark:hover:text-[#E3E3E3] transition-colors rounded-full hover:bg-gray-100 dark:hover:bg-[#333537] flex items-center gap-1"
        title="Select Input Mode"
      >
        <activeMode.icon className="w-5 h-5" />
        <Icons.ChevronDown className={clsx("w-3 h-3 transition-transform", isOpen && "rotate-180")} />
      </button>

      {isOpen && (
        <div className="absolute bottom-full mb-2 left-0 w-56 bg-white dark:bg-[#1E1F20] border border-gray-200 dark:border-[#444746] rounded-xl shadow-xl p-1 z-50 overflow-hidden">
          {modes.map((mode) => (
            <button
              key={mode.id}
              onClick={() => {
                if (mode.id === 'agent') {
                    onSelectAgentMode?.();
                } else {
                    setInputMode(mode.id);
                    // Force exit agent mode if another mode is selected, restoring default behavior
                    setAgentMode(false);
                }
                setIsOpen(false);
              }}
              className={clsx(
                "w-full flex items-center gap-3 px-3 py-2.5 rounded-lg transition-colors text-left text-sm",
                inputMode === mode.id ? "bg-gray-100 dark:bg-[#333537] text-black dark:text-[#E3E3E3] font-medium" : "text-gray-600 dark:text-[#C4C7C5] hover:bg-gray-50 dark:hover:bg-[#333537] hover:text-black dark:hover:text-[#E3E3E3]"
              )}
            >
              <mode.icon className="w-4 h-4" />
              <span>{mode.label}</span>
              {inputMode === mode.id && (
                <Icons.Check className="w-3 h-3 ml-auto" />
              )}
            </button>
          ))}
        </div>
      )}
    </div>
  );
};
