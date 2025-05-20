import { useState } from 'react';
import { Sidebar } from './components/Sidebar';
import { ChatArea } from './components/ChatArea';
import { ChatProvider } from './contexts/ChatContext';
import './App.css';

function App() {
  const [isMobileSidebarOpen, setIsMobileSidebarOpen] = useState(false);

  return (
    <ChatProvider>
      <div className="flex h-screen bg-gray-50">
        {/* Mobile sidebar toggle */}
        <button
          className="md:hidden fixed top-4 left-4 z-20 p-2 bg-silver-500 text-white rounded-md"
          onClick={() => setIsMobileSidebarOpen(!isMobileSidebarOpen)}
        >
          {isMobileSidebarOpen ? '✕' : '☰'}
        </button>

        {/* Sidebar - hidden on mobile unless toggled */}
        <div 
          className={`
            md:relative fixed inset-y-0 left-0 z-10 transform 
            ${isMobileSidebarOpen ? 'translate-x-0' : '-translate-x-full'} 
            md:translate-x-0 transition-transform duration-300 ease-in-out
          `}
        >
          <Sidebar />
        </div>

        {/* Main chat area */}
        <div className="flex-1 flex flex-col overflow-hidden">
          <ChatArea />
        </div>
      </div>
    </ChatProvider>
  );
}

export default App;
