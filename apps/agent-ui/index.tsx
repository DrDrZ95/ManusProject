
import React from 'react';
import { createRoot } from 'react-dom/client';
import { HashRouter, Routes, Route, Navigate } from 'react-router-dom';
import App from './App';
import { ErrorBoundary } from './components/ErrorBoundary';
import { NotFoundPage } from './components/NotFoundPage';
import { ErrorPage } from './components/ErrorPage';

const container = document.getElementById('root');
const root = createRoot(container!);

root.render(
  <React.StrictMode>
    <ErrorBoundary>
      <HashRouter>
        <Routes>
          {/* Main Application Route */}
          <Route path="/" element={<App />} />
          
          {/* Dedicated Error Page Route */}
          <Route path="/error" element={<ErrorPage />} />
          
          {/* 404 Not Found Route */}
          <Route path="/404" element={<NotFoundPage />} />
          
          {/* Catch-all Redirect to 404 */}
          <Route path="*" element={<Navigate to="/404" replace />} />
        </Routes>
      </HashRouter>
    </ErrorBoundary>
  </React.StrictMode>
);
