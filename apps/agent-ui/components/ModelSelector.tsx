import React, { useState, useRef, useEffect } from 'react';
import { useStore } from '../store';
import { Icons } from './icons';
import { ModelType } from '../types';
import { translations } from '../locales';
import clsx from 'clsx';

export const ModelSelector: React.FC = () => {
  const [isOpen, setIsOpen] = useState(false);
  const selectedModel = useStore(s => s.selectedModel);
  const setModel = useStore(s => s.setModel);
  const language = useStore(s => s.language);
  const dropdownRef = useRef<HTMLDivElement>(null);
  
  const t = translations[language];

  const options: { id: ModelType; label: string; icon: any; description: string }[] = [
    { id: 'kimi', label: 'Kimi', icon: Icons.Sparkles, description: t.modelKimi },
    { id: 'deepseek', label: 'Deepseek', icon: Icons.Brain, description: t.modelDeepseek },
    { id: 'gpt-oss', label: 'GPT-OSS', icon: Icons.Box, description: t.modelGptOss },
  ];

  const activeOption = options.find(o => o.id === selectedModel) || options[0];

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
        className="flex items-center gap-2 px-3 py-1.5 rounded-full hover:bg-gray-100 transition-colors text-sm font-medium text-gray-600 hover:text-black"
      >
        <span>{activeOption.label}</span>
        <Icons.ChevronDown className={clsx("w-3 h-3 transition-transform", isOpen && "rotate-180")} />
      </button>

      {isOpen && (
        <div className="absolute bottom-full mb-2 left-0 w-64 bg-white border border-gray-200 rounded-xl shadow-2xl p-1 z-50 overflow-hidden">
          {options.map((option) => (
            <button
              key={option.id}
              onClick={() => {
                setModel(option.id);
                setIsOpen(false);
              }}
              className={clsx(
                "w-full flex items-center gap-3 px-3 py-3 rounded-lg transition-colors text-left",
                selectedModel === option.id ? "bg-gray-50" : "hover:bg-gray-50"
              )}
            >
              <div className={clsx(
                "p-2 rounded-lg",
                selectedModel === option.id ? "bg-black text-white" : "bg-gray-100 text-gray-500"
              )}>
                <option.icon className="w-4 h-4" />
              </div>
              <div>
                <div className={clsx("text-sm font-bold", selectedModel === option.id ? "text-black" : "text-gray-700")}>
                  {option.label}
                </div>
                <div className="text-xs text-gray-500">{option.description}</div>
              </div>
              {selectedModel === option.id && (
                <Icons.Check className="w-4 h-4 text-black ml-auto" />
              )}
            </button>
          ))}
        </div>
      )}
    </div>
  );
};