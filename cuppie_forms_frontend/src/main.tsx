import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import './index.css';
import './tailwindout.css';
import '@/styles/_keyframe-animations.scss';
import '@/styles/_variables.scss';
import App from './App.tsx';
import { AuthProvider } from './context/AuthContext.tsx';
import { MantineProvider } from '@mantine/core';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <MantineProvider
      theme={{}} // можно указать кастомные значения, если нужно
      defaultColorScheme="dark"
    >
      <AuthProvider>
        <App />
      </AuthProvider>
    </MantineProvider>
  </StrictMode>
);



