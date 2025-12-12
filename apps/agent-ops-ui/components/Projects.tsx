import React, { useState } from 'react';
import { 
  GitBranch, MapPin, Play, Clock, Upload, Check, ChevronRight, Terminal as TerminalIcon, 
  MoreVertical, FileText, Search, Plus, X, Loader
} from 'lucide-react';
import { MOCK_PROJECTS, MOCK_COMMITS, TRANSLATIONS } from '../constants';
import { Language, ProjectStatus } from '../types';

interface ProjectsProps {
  lang: Language;
}

const Projects: React.FC<ProjectsProps> = ({ lang }) => {
  const t = TRANSLATIONS[lang];
  const [projects, setProjects] = useState<ProjectStatus[]>(MOCK_PROJECTS);
  const [selectedProject, setSelectedProject] = useState<ProjectStatus | null>(null);
  const [deployStep, setDeployStep] = useState(0);
  const [showDeployModal, setShowDeployModal] = useState(false);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [isCreating, setIsCreating] = useState(false);
  const [cliOpen, setCliOpen] = useState(false);

  // New Project Form State
  const [newProjectName, setNewProjectName] = useState('');
  const [newProjectRepo, setNewProjectRepo] = useState('');
  const [newProjectLocation, setNewProjectLocation] = useState('');

  const startDeploy = (project: ProjectStatus) => {
    setSelectedProject(project);
    setShowDeployModal(true);
    setDeployStep(0);
    
    // Simulate steps
    setTimeout(() => setDeployStep(1), 1500);
    setTimeout(() => setDeployStep(2), 3500);
    setTimeout(() => setDeployStep(3), 5500);
  };

  const handleCreateProject = (e: React.FormEvent) => {
    e.preventDefault();
    if (!newProjectName || !newProjectRepo) return;

    setIsCreating(true);

    // Simulate Network/Processing Delay
    setTimeout(() => {
        const newProject: ProjectStatus = {
        id: Date.now().toString(),
        name: newProjectName,
        status: 'healthy',
        uptime: '0%',
        lastDeployment: 'Never',
        repo: newProjectRepo,
        address: newProjectLocation || 'Unknown',
        branch: 'main'
        };

        setProjects(prev => [newProject, ...prev]);
        setIsCreating(false);
        setShowCreateModal(false);
        setNewProjectName('');
        setNewProjectRepo('');
        setNewProjectLocation('');
        // Optionally auto-select
        setSelectedProject(newProject);
    }, 2000);
  };

  return (
    <div className="h-full pb-20 flex flex-col relative">
       
       {/* Toolbar */}
       <div className="flex justify-between items-center mb-6 bg-light-surface dark:bg-nexus-800 p-4 rounded-lg border border-light-border dark:border-nexus-700 shadow-sm">
          <div className="flex items-center space-x-4">
             <h2 className="text-xl font-bold text-light-text dark:text-white flex items-center">
                <FileText className="mr-2 text-nexus-accent" /> {t.projectCatalog}
             </h2>
             <div className="relative">
                <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-nexus-400" size={14} />
                <input 
                  type="text" 
                  placeholder={t.searchPlaceholder}
                  className="pl-9 pr-4 py-1.5 text-sm bg-nexus-50 dark:bg-nexus-900 border border-light-border dark:border-nexus-700 rounded-full outline-none focus:border-nexus-accent text-light-text dark:text-white"
                />
             </div>
          </div>
          <button 
             onClick={() => setShowCreateModal(true)}
             className="flex items-center px-4 py-2 bg-nexus-accent text-white rounded-lg hover:bg-blue-600 transition-colors shadow-md text-sm font-medium"
          >
             <Plus size={16} className="mr-2" /> New Project
          </button>
       </div>

       {/* Main Layout: List vs Details */}
       <div className="flex-1 flex space-x-6 overflow-hidden">
          
          {/* Project List */}
          <div className="flex-1 overflow-y-auto pr-2">
             <div className="bg-light-surface dark:bg-nexus-800 border border-light-border dark:border-nexus-700 rounded-lg shadow-sm">
                <table className="w-full text-left">
                   <thead className="bg-nexus-50 dark:bg-nexus-900/50 text-xs text-light-textSec dark:text-nexus-400 border-b border-light-border dark:border-nexus-700">
                      <tr>
                         <th className="p-4 font-medium">Project Name</th>
                         <th className="p-4 font-medium">Status</th>
                         <th className="p-4 font-medium">Repository</th>
                         <th className="p-4 font-medium">Location</th>
                         <th className="p-4 font-medium">Actions</th>
                      </tr>
                   </thead>
                   <tbody className="divide-y divide-light-border dark:divide-nexus-700">
                      {projects.map(project => (
                         <tr 
                           key={project.id} 
                           onClick={() => setSelectedProject(project)}
                           className={`cursor-pointer transition-colors ${selectedProject?.id === project.id ? 'bg-nexus-50 dark:bg-nexus-700/50' : 'hover:bg-nexus-50 dark:hover:bg-nexus-800/50'}`}
                         >
                            <td className="p-4 font-medium text-light-text dark:text-white">{project.name}</td>
                            <td className="p-4">
                               <span className={`px-2 py-0.5 rounded text-xs font-bold uppercase ${
                                  project.status === 'healthy' ? 'bg-green-500/10 text-green-500' :
                                  project.status === 'warning' ? 'bg-yellow-500/10 text-yellow-500' :
                                  project.status === 'deploying' ? 'bg-blue-500/10 text-blue-500' : 'bg-red-500/10 text-red-500'
                               }`}>{project.status}</span>
                            </td>
                            <td className="p-4 text-sm text-light-textSec dark:text-nexus-400 font-mono flex items-center">
                               <GitBranch size={14} className="mr-2" /> {project.repo}
                            </td>
                            <td className="p-4 text-sm text-light-textSec dark:text-nexus-400">
                               <div className="flex items-center">
                                  <MapPin size={14} className="mr-2 text-nexus-500" />
                                  {project.address}
                               </div>
                            </td>
                            <td className="p-4">
                               <button 
                                 onClick={(e) => { e.stopPropagation(); startDeploy(project); }}
                                 className="p-2 bg-nexus-100 dark:bg-nexus-700 text-nexus-600 dark:text-nexus-300 rounded hover:bg-nexus-accent hover:text-white transition-colors"
                               >
                                  <Play size={16} />
                               </button>
                            </td>
                         </tr>
                      ))}
                   </tbody>
                </table>
             </div>
          </div>

          {/* Details Panel */}
          {selectedProject && (
             <div className="w-96 bg-light-surface dark:bg-nexus-800 border border-light-border dark:border-nexus-700 rounded-lg shadow-sm flex flex-col animate-fade-in overflow-hidden">
                <div className="p-4 border-b border-light-border dark:border-nexus-700 bg-nexus-50 dark:bg-nexus-900/50">
                   <h3 className="font-bold text-light-text dark:text-white">{selectedProject.name}</h3>
                   <div className="flex items-center text-xs text-light-textSec dark:text-nexus-400 mt-1">
                      <span className="bg-nexus-200 dark:bg-nexus-700 px-1.5 py-0.5 rounded mr-2 text-light-text dark:text-white">{selectedProject.branch}</span>
                      <span>Last update: 2 mins ago</span>
                   </div>
                </div>
                
                <div className="flex-1 overflow-y-auto p-4 space-y-6">
                   {/* Map Preview Placeholder */}
                   <div>
                      <h4 className="text-xs font-bold uppercase text-light-textSec dark:text-nexus-500 mb-2">{t.physicalAddress}</h4>
                      <div className="h-32 bg-nexus-100 dark:bg-nexus-900 rounded-lg border border-light-border dark:border-nexus-700 flex items-center justify-center relative overflow-hidden group">
                         {/* Abstract Map Background */}
                         <div className="absolute inset-0 opacity-20 bg-[url('https://upload.wikimedia.org/wikipedia/commons/e/ec/World_map_blank_without_borders.svg')] bg-cover"></div>
                         <MapPin size={32} className="text-red-500 z-10 drop-shadow-lg" />
                         <div className="absolute bottom-2 left-2 bg-black/70 text-white text-[10px] px-2 py-1 rounded backdrop-blur">
                            {selectedProject.address}
                         </div>
                      </div>
                   </div>

                   {/* Git History Timeline */}
                   <div>
                      <h4 className="text-xs font-bold uppercase text-light-textSec dark:text-nexus-500 mb-3">{t.gitHistory}</h4>
                      <div className="relative pl-4 border-l-2 border-nexus-200 dark:border-nexus-700 space-y-6">
                         {MOCK_COMMITS.map(commit => (
                            <div key={commit.id} className="relative">
                               <div className="absolute -left-[21px] top-0 w-3 h-3 rounded-full bg-nexus-400 border-2 border-white dark:border-nexus-800"></div>
                               <div className="text-sm font-medium text-light-text dark:text-white">{commit.message}</div>
                               <div className="text-xs text-light-textSec dark:text-nexus-400 flex justify-between mt-1">
                                  <span>{commit.author}</span>
                                  <span className="font-mono">{commit.hash}</span>
                               </div>
                            </div>
                         ))}
                      </div>
                   </div>

                   {/* Upload Artifacts */}
                   <div className="border-2 border-dashed border-light-border dark:border-nexus-700 rounded-lg p-4 text-center hover:border-nexus-accent transition-colors cursor-pointer">
                      <Upload size={24} className="mx-auto text-nexus-400 mb-2" />
                      <div className="text-xs text-light-textSec dark:text-nexus-400">Drag files to upload artifacts</div>
                   </div>
                </div>

                {/* Bottom CLI Drawer Trigger */}
                <button 
                  onClick={() => setCliOpen(!cliOpen)}
                  className="p-3 bg-nexus-900 text-nexus-300 text-xs font-mono border-t border-nexus-700 flex items-center justify-between hover:text-white"
                >
                   <span className="flex items-center"><TerminalIcon size={14} className="mr-2" /> git push origin {selectedProject.branch}</span>
                   <ChevronRight size={14} className={`transform transition-transform ${cliOpen ? 'rotate-90' : ''}`} />
                </button>
             </div>
          )}
       </div>
       
       {/* Create Project Modal */}
       {showCreateModal && (
          <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4 animate-fade-in">
             <div className="bg-light-surface dark:bg-nexus-800 rounded-xl shadow-2xl w-full max-w-md border border-light-border dark:border-nexus-700 overflow-hidden">
                <div className="p-4 border-b border-light-border dark:border-nexus-700 flex justify-between items-center bg-nexus-50 dark:bg-nexus-900/50">
                   <h3 className="text-lg font-bold text-light-text dark:text-white">{t.createProject}</h3>
                   {!isCreating && (
                      <button onClick={() => setShowCreateModal(false)} className="text-nexus-400 hover:text-nexus-900 dark:hover:text-white">
                         <X size={20} />
                      </button>
                   )}
                </div>
                <form onSubmit={handleCreateProject} className="p-6 space-y-4">
                   {isCreating ? (
                      <div className="flex flex-col items-center justify-center py-8">
                         <Loader size={48} className="text-nexus-accent animate-spin mb-4" />
                         <p className="text-light-text dark:text-white font-medium">{t.creating}</p>
                         <p className="text-xs text-light-textSec dark:text-nexus-400 mt-2">Initializing Git repository...</p>
                      </div>
                   ) : (
                      <>
                         <div>
                            <label className="block text-sm font-medium text-light-text dark:text-nexus-300 mb-1">{t.projectName}</label>
                            <input 
                               type="text" 
                               required
                               value={newProjectName}
                               onChange={(e) => setNewProjectName(e.target.value)}
                               className="w-full bg-nexus-50 dark:bg-nexus-900 border border-light-border dark:border-nexus-600 rounded-lg p-2.5 text-sm text-light-text dark:text-white focus:border-nexus-accent outline-none"
                               placeholder="e.g. My-Microservice-V2"
                            />
                         </div>
                         <div>
                            <label className="block text-sm font-medium text-light-text dark:text-nexus-300 mb-1">{t.repositoryUrl}</label>
                            <input 
                               type="text" 
                               required
                               value={newProjectRepo}
                               onChange={(e) => setNewProjectRepo(e.target.value)}
                               className="w-full bg-nexus-50 dark:bg-nexus-900 border border-light-border dark:border-nexus-600 rounded-lg p-2.5 text-sm text-light-text dark:text-white focus:border-nexus-accent outline-none"
                               placeholder="git/my-repo"
                            />
                         </div>
                         <div>
                            <label className="block text-sm font-medium text-light-text dark:text-nexus-300 mb-1">{t.physicalAddress}</label>
                            <input 
                               type="text" 
                               value={newProjectLocation}
                               onChange={(e) => setNewProjectLocation(e.target.value)}
                               className="w-full bg-nexus-50 dark:bg-nexus-900 border border-light-border dark:border-nexus-600 rounded-lg p-2.5 text-sm text-light-text dark:text-white focus:border-nexus-accent outline-none"
                               placeholder="e.g. New York, DataCenter-1"
                            />
                         </div>
                         <div className="pt-2 flex justify-end space-x-3">
                            <button 
                               type="button" 
                               onClick={() => setShowCreateModal(false)}
                               className="px-4 py-2 text-sm text-light-textSec dark:text-nexus-300 hover:bg-nexus-100 dark:hover:bg-nexus-700 rounded-lg"
                            >
                               Cancel
                            </button>
                            <button 
                               type="submit"
                               className="px-4 py-2 text-sm bg-nexus-accent text-white rounded-lg hover:bg-blue-600 font-medium"
                            >
                               {t.create}
                            </button>
                         </div>
                      </>
                   )}
                </form>
             </div>
          </div>
       )}

       {/* Deployment Modal Overlay */}
       {showDeployModal && selectedProject && (
          <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4">
             <div className="bg-light-surface dark:bg-nexus-800 rounded-xl shadow-2xl w-full max-w-lg border border-light-border dark:border-nexus-700 overflow-hidden animate-slide-up">
                <div className="p-6 border-b border-light-border dark:border-nexus-700 flex justify-between items-center bg-nexus-50 dark:bg-nexus-900/50">
                   <h3 className="text-lg font-bold text-light-text dark:text-white">{t.deploy}: {selectedProject.name}</h3>
                </div>
                <div className="p-8">
                   <div className="flex items-center justify-between relative mb-8">
                      <div className="absolute top-1/2 left-0 w-full h-1 bg-nexus-200 dark:bg-nexus-700 -z-0"></div>
                      {['Clone', 'Build', 'Test', 'Deploy'].map((step, idx) => (
                         <div key={step} className="relative z-10 flex flex-col items-center">
                            <div className={`w-10 h-10 rounded-full flex items-center justify-center border-4 transition-colors ${
                               deployStep > idx ? 'bg-green-500 border-green-500 text-white' :
                               deployStep === idx ? 'bg-white dark:bg-nexus-800 border-blue-500 text-blue-500 animate-pulse' :
                               'bg-nexus-200 dark:bg-nexus-700 border-nexus-300 dark:border-nexus-600 text-nexus-500'
                            }`}>
                               {deployStep > idx ? <Check size={16} /> : <span>{idx + 1}</span>}
                            </div>
                            <span className="mt-2 text-xs font-bold text-light-textSec dark:text-nexus-400">{step}</span>
                         </div>
                      ))}
                   </div>
                   
                   <div className="bg-black rounded-lg p-4 font-mono text-xs text-nexus-300 h-32 overflow-y-auto mb-6">
                      {deployStep >= 0 && <div className="text-green-400">➜ git clone {selectedProject.repo}...</div>}
                      {deployStep >= 1 && <div className="text-green-400">➜ npm run build... <span className="text-white">Done in 2.4s</span></div>}
                      {deployStep >= 2 && <div className="text-green-400">➜ running tests... <span className="text-white">Passed (142/142)</span></div>}
                      {deployStep >= 3 && <div className="text-blue-400">➜ deploying to production... Success!</div>}
                   </div>
                   
                   <div className="flex justify-end">
                      <button 
                         onClick={() => { setShowDeployModal(false); setDeployStep(0); }}
                         className="px-6 py-2 bg-nexus-200 dark:bg-nexus-700 text-light-text dark:text-white rounded hover:bg-nexus-300 dark:hover:bg-nexus-600 transition-colors font-medium"
                      >
                         {deployStep === 3 ? 'Close' : 'Cancel'}
                      </button>
                   </div>
                </div>
             </div>
          </div>
       )}

       {/* Bottom CLI Drawer */}
       {cliOpen && selectedProject && (
          <div className="absolute bottom-0 left-0 right-0 h-48 bg-nexus-900 border-t border-nexus-700 p-4 font-mono text-xs text-nexus-300 z-10 overflow-y-auto shadow-2xl animate-slide-up">
             <div className="flex items-center text-green-400 mb-2">
                <span className="mr-2">➜</span>
                <span>{selectedProject.name} git:(<span className="text-blue-400">{selectedProject.branch}</span>)</span>
             </div>
             <div className="flex items-center">
                <span className="mr-2 text-nexus-500">$</span>
                <input type="text" className="bg-transparent outline-none text-white w-full" placeholder="Type git command..." autoFocus />
             </div>
          </div>
       )}
    </div>
  );
};

export default Projects;