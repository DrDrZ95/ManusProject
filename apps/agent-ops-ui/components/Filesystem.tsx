
import React, { useState } from 'react';
import { 
  Folder, File, HardDrive, ChevronRight, ChevronDown, Search, MoreHorizontal, 
  Terminal as TerminalIcon, PieChart as PieIcon, RefreshCw, HardDriveDownload 
} from 'lucide-react';
import { Treemap, ResponsiveContainer, Tooltip } from 'recharts';
import { MOCK_FILESYSTEM, TRANSLATIONS } from '../constants';
import { Language, FileSystemNode } from '../types';

interface FilesystemProps {
  lang: Language;
}

interface TreeItemProps {
  node: FileSystemNode;
  level: number;
  onSelect: (n: FileSystemNode) => void;
}

// Custom Tree Item Component
const TreeItem: React.FC<TreeItemProps> = ({ node, level, onSelect }) => {
  const [expanded, setExpanded] = useState(true);
  
  const Icon = node.type === 'drive' ? HardDrive : node.type === 'folder' ? Folder : File;
  const color = node.type === 'drive' ? 'text-blue-500' : node.type === 'folder' ? 'text-yellow-500' : 'text-nexus-400';

  return (
    <div>
       <div 
         className={`flex items-center py-1 px-2 hover:bg-nexus-100 dark:hover:bg-nexus-800 cursor-pointer text-sm`}
         style={{ paddingLeft: `${level * 16 + 8}px` }}
         onClick={() => { onSelect(node); if (node.children) setExpanded(!expanded); }}
       >
          <div className="mr-1 w-4 flex justify-center text-nexus-400">
             {node.children && (
                expanded ? <ChevronDown size={12} /> : <ChevronRight size={12} />
             )}
          </div>
          <Icon size={16} className={`mr-2 ${color}`} />
          <span className="text-light-text dark:text-white truncate">{node.name}</span>
       </div>
       {expanded && node.children && node.children.map(child => (
          <TreeItem key={child.id} node={child} level={level + 1} onSelect={onSelect} />
       ))}
    </div>
  );
};

const Filesystem: React.FC<FilesystemProps> = ({ lang }) => {
  const t = TRANSLATIONS[lang];
  const [selectedNode, setSelectedNode] = useState<FileSystemNode>(MOCK_FILESYSTEM[0]);
  const [termInput, setTermInput] = useState('');

  // Transform data for Treemap
  const treeMapData = [
     { name: 'models', size: 450, fill: '#3b82f6' },
     { name: 'datasets', size: 600, fill: '#a855f7' },
     { name: 'logs', size: 150, fill: '#f59e0b' },
     { name: 'system', size: 50, fill: '#64748b' }
  ];

  const CustomTreemapContent = (props: any) => {
    const { root, depth, x, y, width, height, index, name } = props;
    return (
      <g>
        <rect
          x={x}
          y={y}
          width={width}
          height={height}
          style={{
            fill: props.fill,
            stroke: '#1e293b',
            strokeWidth: 2 / (depth + 1e-10),
            strokeOpacity: 1 / (depth + 1e-10),
          }}
        />
        {width > 30 && height > 30 && (
          <text x={x + width / 2} y={y + height / 2 + 7} textAnchor="middle" fill="#fff" fontSize={12}>
            {name}
          </text>
        )}
      </g>
    );
  };

  return (
    <div className="h-full flex flex-col pb-20 space-y-6">
       
       {/* Actions Bar */}
       <div className="flex justify-between items-center bg-light-surface dark:bg-nexus-800 p-4 rounded-lg border border-light-border dark:border-nexus-700 shadow-sm">
          <div className="flex items-center space-x-4">
             <h2 className="text-xl font-bold text-light-text dark:text-white flex items-center">
                <HardDrive className="mr-2 text-nexus-accent" /> {t.filesystem}
             </h2>
             <div className="flex space-x-2">
                <button className="p-2 hover:bg-nexus-100 dark:hover:bg-nexus-700 rounded text-nexus-500 dark:text-nexus-300" title={t.backup}>
                   <HardDriveDownload size={18} />
                </button>
                <button className="p-2 hover:bg-nexus-100 dark:hover:bg-nexus-700 rounded text-nexus-500 dark:text-nexus-300" title={t.syncFiles}>
                   <RefreshCw size={18} />
                </button>
             </div>
          </div>
          <div className="relative w-64">
             <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-nexus-400" size={14} />
             <input 
               type="text" 
               placeholder="Find files..."
               className="w-full pl-9 pr-4 py-1.5 text-sm bg-nexus-50 dark:bg-nexus-900 border border-light-border dark:border-nexus-700 rounded-full outline-none focus:border-nexus-accent"
             />
          </div>
       </div>

       {/* Main Split View */}
       <div className="flex-1 flex space-x-6 min-h-[400px]">
          
          {/* File Tree (Left) */}
          <div className="w-1/4 bg-light-surface dark:bg-nexus-800 border border-light-border dark:border-nexus-700 rounded-lg shadow-sm flex flex-col overflow-hidden">
             <div className="p-3 bg-nexus-50 dark:bg-nexus-900/50 border-b border-light-border dark:border-nexus-700 font-bold text-xs text-light-textSec dark:text-nexus-400 uppercase tracking-wider">
                {t.fileExplorer}
             </div>
             <div className="flex-1 overflow-y-auto py-2">
                {MOCK_FILESYSTEM.map(node => (
                   <TreeItem key={node.id} node={node} level={0} onSelect={setSelectedNode} />
                ))}
             </div>
          </div>

          {/* Visualization & Preview (Right) */}
          <div className="flex-1 flex flex-col space-y-6">
             
             {/* Disk Usage Treemap */}
             <div className="h-64 bg-light-surface dark:bg-nexus-800 border border-light-border dark:border-nexus-700 rounded-lg p-4 shadow-sm relative">
                <div className="absolute top-4 left-4 z-10 flex items-center text-sm font-bold text-light-text dark:text-white">
                   <PieIcon className="mr-2 text-nexus-accent" size={16} /> {t.diskUsage}
                </div>
                <ResponsiveContainer width="100%" height="100%">
                   <Treemap
                      data={treeMapData}
                      dataKey="size"
                      aspectRatio={4 / 3}
                      stroke="#fff"
                      fill="#8884d8"
                      content={<CustomTreemapContent />}
                   >
                      <Tooltip 
                        contentStyle={{ backgroundColor: '#1e293b', border: 'none', color: '#fff' }}
                        formatter={(value: any) => `${value} GB`}
                      />
                   </Treemap>
                </ResponsiveContainer>
             </div>

             {/* Selected Info / Terminal */}
             <div className="flex-1 bg-nexus-900 border border-nexus-700 rounded-lg shadow-inner flex flex-col overflow-hidden font-mono text-sm">
                <div className="bg-nexus-800 px-4 py-2 border-b border-nexus-700 flex justify-between items-center text-nexus-400 text-xs">
                   <div className="flex items-center">
                      <TerminalIcon size={14} className="mr-2" /> 
                      root@agentproject:{selectedNode.mountPoint || selectedNode.name}
                   </div>
                   <div>{selectedNode.permissions} | {selectedNode.size}</div>
                </div>
                
                <div className="flex-1 p-4 text-nexus-300 overflow-y-auto">
                   <div className="mb-4">
                      {selectedNode.type === 'file' ? (
                         <div className="bg-black/30 p-4 rounded border border-nexus-700/50">
                            <div className="text-nexus-500 mb-2"># File Content Preview</div>
                            <pre className="text-green-400 font-mono whitespace-pre-wrap">{selectedNode.content}</pre>
                         </div>
                      ) : (
                         <div className="grid grid-cols-4 gap-4">
                            {selectedNode.children?.map(child => (
                               <div key={child.id} className="flex flex-col items-center p-4 hover:bg-nexus-800 rounded cursor-pointer">
                                  {child.type === 'folder' ? <Folder size={32} className="text-yellow-500 mb-2"/> : <File size={32} className="text-nexus-400 mb-2"/>}
                                  <span className="text-xs truncate w-full text-center">{child.name}</span>
                               </div>
                            ))}
                         </div>
                      )}
                   </div>
                   
                   {/* CLI Input */}
                   <div className="flex items-center mt-auto border-t border-nexus-800 pt-4">
                      <span className="text-green-500 font-bold mr-2">➜</span>
                      <span className="text-blue-400 font-bold mr-2">{selectedNode.name}</span>
                      <input 
                        type="text" 
                        value={termInput}
                        onChange={(e) => setTermInput(e.target.value)}
                        className="flex-1 bg-transparent border-none outline-none text-white"
                        placeholder="ls -la" 
                      />
                   </div>
                </div>
             </div>
          </div>
       </div>
    </div>
  );
};

export default Filesystem;
