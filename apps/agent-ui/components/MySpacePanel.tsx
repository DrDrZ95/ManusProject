
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

const MOCK_SOLUTIONS = [
  { id: 's1', name: 'Auto-Scaling Infrastructure', type: 'doc', size: 'Solution', date: '2025-05-14', content: 'Implements K8s HPA.' },
  { id: 's2', name: 'Auth0 Integration Flow', type: 'code', size: 'Solution', date: '2025-05-13', content: 'Redirect to /callback...' },
  { id: 's3', name: 'Database Migration Strategy', type: 'doc', size: 'Solution', date: '2025-05-11', content: 'Blue/Green deployment strategy.' },
];

export const MySpacePanel: React.FC = () => {
  const isTerminalOpen = useStore((s) => s.isTerminalOpen);
  const toggleTerminal = useStore((s) => s.toggleTerminal);
  const language = useStore(s => s.language);
  const t = translations[language];

  const [activeTab, setActiveTab] = useState<'files' | 'solutions'>('files');
  const [previewFile, setPreviewFile] = useState<typeof MOCK_FILES[0] | null>(null);
  const [selectedIds, setSelectedIds] = useState<string[]>([]);
  const [files, setFiles] = useState(MOCK_FILES);
  const [solutions, setSolutions] = useState(MOCK_SOLUTIONS);
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);

  if (!isTerminalOpen) return null;

  const handleRemoteClick = () => {
    alert(t.functionUnderDev);
  };

  const toggleSelection = (id: string) => {
    setSelectedIds(prev => 
      prev.includes(id) 
        ? prev.filter(i => i !== id) 
        : [...prev, id]
    );
  };

  const handleDoubleClick = (file: typeof MOCK_FILES[0]) => {
    setPreviewFile(file);
  };

  // Triggered via Hover Button
  const handlePreviewClick = (e: React.MouseEvent, file: typeof MOCK_FILES[0]) => {
    e.stopPropagation();
    setPreviewFile(file);
  };

  // Triggered via Hover Button
  const handleDownloadClick = (e: React.MouseEvent, file: typeof MOCK_FILES[0]) => {
    e.stopPropagation();
    // Simulate download
    const link = document.createElement('a');
    link.href = '#';
    link.download = file.name;
    // visual feedback
    alert(`Downloading ${file.name}... (Simulation)`);
  };

  const handleDeleteRequest = () => {
    setShowDeleteConfirm(true);
  };

  const confirmDelete = () => {
    if (activeTab === 'files') {
        setFiles(prev => prev.filter(f => !selectedIds.includes(f.id)));
    } else {
        setSolutions(prev => prev.filter(f => !selectedIds.includes(f.id)));
    }
    setSelectedIds([]);
    setShowDeleteConfirm(false);
  };

  const cancelDelete = () => {
    setShowDeleteConfirm(false);
  };

  const getFileIcon = (type: string) => {
    switch (type) {
      case 'image': return <Icons.FileImage className="w-5 h-5 text-purple-500" />;
      case 'code': return <Icons.FileCode className="w-5 h-5 text-blue-500" />;
      default: return <Icons.FileText className="w-5 h-5 text-gray-500" />;
    }
  };

  const currentList = activeTab === 'files' ? files : solutions;

  return (
    <div className="h-full w-full flex flex-col bg-white relative overflow-hidden border-l border-gray-200 shadow-xl">
      {/* Header */}
      <div className="flex items-center justify-between px-4 py-3 bg-gray-50 border-b border-gray-200 flex-shrink-0">
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
      <div className="flex-1 overflow-y-auto p-2 bg-white custom-scrollbar relative pb-16">
         <div className="space-y-1">
            {currentList.map((file) => {
              const isSelected = selectedIds.includes(file.id);
              return (
                <motion.div 
                  key={file.id}
                  layoutId={`file-${file.id}`}
                  onClick={() => toggleSelection(file.id)}
                  onDoubleClick={() => handleDoubleClick(file)}
                  className={clsx(
                    "group flex items-center p-3 rounded-xl cursor-pointer border transition-all select-none relative",
                    isSelected 
                      ? "bg-blue-50 border-blue-200 shadow-sm" 
                      : "bg-transparent hover:bg-gray-50 border-transparent hover:border-gray-100"
                  )}
                >
                   <div className={clsx(
                     "p-3 rounded-lg border shadow-sm mr-3 transition-colors",
                     isSelected ? "bg-white border-blue-100" : "bg-gray-50 border-gray-100 group-hover:bg-white group-hover:border-gray-200"
                   )}>
                      {getFileIcon(file.type)}
                   </div>
                   <div className="flex-1 min-w-0">
                      <div className={clsx("text-sm font-medium truncate transition-colors", isSelected ? "text-blue-700" : "text-gray-900")}>
                        {file.name}
                      </div>
                      <div className="flex items-center text-[10px] text-gray-400 gap-2 mt-0.5">
                         <span>{file.size}</span>
                         <span className="w-0.5 h-0.5 bg-gray-300 rounded-full" />
                         <span>{file.date}</span>
                      </div>
                   </div>
                   
                   {/* Hover Actions (Preview & Download) */}
                   {!isSelected && (
                     <div className="absolute right-3 flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity bg-white/80 backdrop-blur-sm rounded-lg p-1 shadow-sm border border-gray-100">
                        <button 
                          onClick={(e) => handlePreviewClick(e, file)}
                          className="p-1.5 text-gray-500 hover:text-black hover:bg-gray-100 rounded-md transition-colors"
                          title={t.previewFile}
                        >
                           <Icons.Preview className="w-4 h-4" />
                        </button>
                        <div className="w-px h-3 bg-gray-300" />
                        <button 
                          onClick={(e) => handleDownloadClick(e, file)}
                          className="p-1.5 text-gray-500 hover:text-black hover:bg-gray-100 rounded-md transition-colors"
                          title={t.download}
                        >
                           <Icons.Download className="w-4 h-4" />
                        </button>
                     </div>
                   )}

                   {isSelected && (
                      <div className="flex items-center justify-center pl-2">
                         <Icons.Check className="w-5 h-5 text-blue-600" />
                      </div>
                   )}
                </motion.div>
              );
            })}
            {currentList.length === 0 && (
              <div className="text-center text-gray-400 text-sm mt-10">
                No items available.
              </div>
            )}
         </div>
      </div>
      
      {/* Tabs Bottom */}
      <div className="flex-shrink-0 border-t border-gray-200 bg-gray-50 p-1 flex">
         <button 
            onClick={() => { setActiveTab('files'); setSelectedIds([]); }}
            className={clsx(
                "flex-1 py-2.5 text-xs font-bold uppercase tracking-wider text-center rounded-lg transition-all",
                activeTab === 'files' ? "bg-white text-black shadow-sm" : "text-gray-500 hover:bg-gray-200 hover:text-black"
            )}
         >
            {t.mySpaceFiles}
         </button>
         <button 
            onClick={() => { setActiveTab('solutions'); setSelectedIds([]); }}
            className={clsx(
                "flex-1 py-2.5 text-xs font-bold uppercase tracking-wider text-center rounded-lg transition-all",
                activeTab === 'solutions' ? "bg-white text-black shadow-sm" : "text-gray-500 hover:bg-gray-200 hover:text-black"
            )}
         >
            {t.mySpaceSolutions}
         </button>
      </div>

      {/* Batch Actions Bar */}
      <AnimatePresence>
        {selectedIds.length > 0 && (
          <motion.div 
            initial={{ y: 100, opacity: 0 }}
            animate={{ y: 0, opacity: 1 }}
            exit={{ y: 100, opacity: 0 }}
            className="absolute bottom-16 left-4 right-4 bg-white border border-gray-200 shadow-2xl rounded-xl p-3 z-40 flex items-center justify-between"
          >
             <div className="flex items-center gap-2 px-2">
                <span className="bg-black text-white text-xs font-bold px-2 py-0.5 rounded-md">{selectedIds.length}</span>
                <span className="text-sm font-medium text-gray-600">{t.itemsSelected}</span>
             </div>
             <button 
               onClick={handleDeleteRequest}
               className="flex items-center gap-2 px-4 py-2 bg-red-50 text-red-600 hover:bg-red-100 rounded-lg text-sm font-medium transition-colors"
             >
                <Icons.Trash className="w-4 h-4" />
                {t.deleteSelected}
             </button>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Delete Confirmation Modal */}
      <AnimatePresence>
        {showDeleteConfirm && (
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="absolute inset-0 z-50 bg-black/20 backdrop-blur-sm flex items-center justify-center p-6"
          >
             <motion.div
                initial={{ scale: 0.9, y: 20 }}
                animate={{ scale: 1, y: 0 }}
                exit={{ scale: 0.9, y: 20 }}
                className="bg-white rounded-2xl shadow-xl p-6 w-full max-w-sm border border-gray-200"
             >
                <div className="w-12 h-12 bg-red-100 text-red-500 rounded-full flex items-center justify-center mb-4 mx-auto">
                   <Icons.Trash className="w-6 h-6" />
                </div>
                <h3 className="text-lg font-bold text-center text-gray-900 mb-2">Delete {selectedIds.length} Item(s)?</h3>
                <p className="text-center text-gray-500 text-sm mb-6 leading-relaxed">
                  {t.confirmBatchDelete}
                </p>
                <div className="flex gap-3">
                   <button 
                     onClick={cancelDelete}
                     className="flex-1 px-4 py-2.5 bg-gray-100 text-gray-700 font-medium rounded-xl hover:bg-gray-200 transition-colors"
                   >
                     Cancel
                   </button>
                   <button 
                     onClick={confirmDelete}
                     className="flex-1 px-4 py-2.5 bg-red-500 text-white font-medium rounded-xl hover:bg-red-600 transition-colors shadow-lg shadow-red-200"
                   >
                     Delete
                   </button>
                </div>
             </motion.div>
          </motion.div>
        )}
      </AnimatePresence>

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
