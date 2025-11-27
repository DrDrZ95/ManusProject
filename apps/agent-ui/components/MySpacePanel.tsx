
import React, { useState } from 'react';
import { Icons } from './icons';
import { useStore } from '../store';
import { translations } from '../locales';
import { motion, AnimatePresence } from 'framer-motion';
import clsx from 'clsx';

// Mock Files Data
const MOCK_FILES = [
  { id: '1', name: 'project_architecture.png', type: 'image', size: '2.4 MB', date: '2025-05-12', content: 'https://picsum.photos/seed/arch/800/600' },
  { id: '2', name: 'meeting_notes_q2.docx', type: 'doc', size: '145 KB', date: '2025-05-10', content: 'Q2 Planning Meeting\n\n- Goals achieved: 85%\n- Next steps: Focus on mobile optimization.' },
  { id: '3', name: 'api_endpoints.md', type: 'doc', size: '12 KB', date: '2025-05-08', content: '# API Endpoints\n\n## GET /users\nReturns list of users.\n\n## POST /login\nAuthenticates user.' },
  { id: '4', name: 'styles.css', type: 'code', size: '45 KB', date: '2025-05-05', content: '.container { max-width: 1200px; margin: 0 auto; }' },
  { id: '5', name: 'avatar_draft.jpg', type: 'image', size: '1.2 MB', date: '2025-05-01', content: 'https://picsum.photos/seed/anime/400/400' },
];

export const MySpacePanel: React.FC = () => {
  const isTerminalOpen = useStore((s) => s.isTerminalOpen); // Reusing isTerminalOpen for right panel visibility
  const toggleTerminal = useStore((s) => s.toggleTerminal);
  const language = useStore(s => s.language);
  const t = translations[language];

  const [previewFile, setPreviewFile] = useState<typeof MOCK_FILES[0] | null>(null);

  if (!isTerminalOpen) return null;

  const handleRemoteClick = () => {
    alert(t.functionUnderDev);
  };

  const getFileIcon = (type: string) => {
    switch (type) {
      case 'image': return <Icons.FileImage className="w-5 h-5 text-purple-500" />;
      case 'code': return <Icons.FileCode className="w-5 h-5 text-blue-500" />;
      default: return <Icons.FileText className="w-5 h-5 text-gray-500" />;
    }
  };

  return (
    <div className="h-full w-full flex flex-col bg-white relative overflow-hidden border-l border-gray-200">
      {/* Header */}
      <div className="flex items-center justify-between px-4 py-3 bg-gray-50 border-b border-gray-200">
        <div className="flex items-center space-x-2 select-none">
          <div className="w-7 h-7 bg-black text-white rounded-lg flex items-center justify-center">
             <Icons.Folder className="w-4 h-4" />
          </div>
          <span className="font-bold text-gray-900">{t.mySpace}</span>
        </div>
        
        <div className="flex items-center gap-2">
           {/* Remote Linux Button */}
           <button 
             onClick={handleRemoteClick}
             className="flex items-center gap-1.5 px-2 py-1.5 text-xs font-medium text-gray-600 hover:text-black hover:bg-gray-200 rounded-md transition-colors"
             title={t.functionUnderDev}
           >
             <Icons.Remote className="w-3.5 h-3.5" />
             <span className="hidden sm:inline">{t.remoteLinux}</span>
           </button>

           <div className="w-px h-4 bg-gray-300 mx-1" />

           <button onClick={toggleTerminal} className="p-1.5 text-gray-400 hover:text-black transition-colors rounded-md hover:bg-gray-200">
            <Icons.ClosePanelRight className="w-4 h-4" />
          </button>
        </div>
      </div>

      {/* File List */}
      <div className="flex-1 overflow-y-auto p-2 bg-white custom-scrollbar">
         <div className="space-y-1">
            {MOCK_FILES.map((file) => (
              <motion.div 
                key={file.id}
                layoutId={`file-${file.id}`}
                onClick={() => setPreviewFile(file)}
                className="group flex items-center p-3 rounded-xl hover:bg-gray-50 cursor-pointer border border-transparent hover:border-gray-100 transition-all"
              >
                 <div className="p-3 bg-gray-50 group-hover:bg-white rounded-lg border border-gray-100 group-hover:border-gray-200 shadow-sm mr-3 transition-colors">
                    {getFileIcon(file.type)}
                 </div>
                 <div className="flex-1 min-w-0">
                    <div className="text-sm font-medium text-gray-900 truncate group-hover:text-black transition-colors">{file.name}</div>
                    <div className="flex items-center text-[10px] text-gray-400 gap-2 mt-0.5">
                       <span>{file.size}</span>
                       <span className="w-0.5 h-0.5 bg-gray-300 rounded-full" />
                       <span>{file.date}</span>
                    </div>
                 </div>
                 <div className="opacity-0 group-hover:opacity-100 transition-opacity flex items-center gap-1">
                    <button className="p-2 text-gray-400 hover:text-black rounded-full hover:bg-gray-200">
                       <Icons.Preview className="w-4 h-4" />
                    </button>
                    <button className="p-2 text-gray-400 hover:text-black rounded-full hover:bg-gray-200">
                       <Icons.Download className="w-4 h-4" />
                    </button>
                 </div>
              </motion.div>
            ))}
         </div>
      </div>

      {/* Preview Modal */}
      <AnimatePresence>
        {previewFile && (
          <motion.div 
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="absolute inset-0 z-50 bg-white/60 backdrop-blur-md p-4 flex flex-col"
          >
             <motion.div 
               initial={{ scale: 0.95, y: 10 }}
               animate={{ scale: 1, y: 0 }}
               exit={{ scale: 0.95, y: 10 }}
               className="flex-1 bg-white rounded-2xl shadow-2xl border border-gray-200 flex flex-col overflow-hidden ring-1 ring-black/5"
             >
                {/* Preview Header */}
                <div className="flex items-center justify-between px-4 py-3 border-b border-gray-100 bg-gray-50/50">
                   <div className="flex items-center gap-3 overflow-hidden">
                      <button onClick={() => setPreviewFile(null)} className="p-1.5 hover:bg-gray-200 rounded-full transition-colors text-gray-500 hover:text-black">
                         <Icons.ChevronDown className="w-5 h-5 rotate-90" />
                      </button>
                      <span className="font-semibold text-sm truncate text-gray-900">{previewFile.name}</span>
                   </div>
                   <button className="px-3 py-1.5 bg-black text-white hover:bg-gray-800 rounded-lg flex items-center gap-2 text-xs font-medium transition-all shadow-sm">
                      <Icons.Download className="w-3.5 h-3.5" />
                      {t.download}
                   </button>
                </div>

                {/* Preview Content */}
                <div className="flex-1 bg-gray-100/50 p-6 overflow-y-auto flex items-center justify-center relative">
                   {previewFile.type === 'image' ? (
                      <img src={previewFile.content} alt="Preview" className="max-w-full max-h-full rounded-lg shadow-lg border border-gray-200 object-contain" />
                   ) : (
                      <div className="bg-white p-8 rounded-xl shadow-lg border border-gray-200 max-w-md w-full">
                         <div className="flex justify-center mb-6">
                            <div className="p-4 bg-gray-50 rounded-2xl border border-gray-100">
                               {getFileIcon(previewFile.type)}
                            </div>
                         </div>
                         <div className="text-center">
                            <div className="text-left bg-gray-50 p-4 rounded-lg text-xs font-mono text-gray-600 overflow-auto max-h-60 border border-gray-200 shadow-inner">
                                <pre className="whitespace-pre-wrap">{previewFile.content}</pre>
                            </div>
                         </div>
                      </div>
                   )}
                </div>
             </motion.div>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
};
