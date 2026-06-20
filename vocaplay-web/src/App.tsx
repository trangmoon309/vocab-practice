import { Navigate, Route, Routes } from 'react-router-dom'
import { ProtectedRoute } from './components/layout/ProtectedRoute'
import { LoginPage } from './pages/LoginPage'
import { RegisterPage } from './pages/RegisterPage'
import { WordSetsPage } from './pages/WordSetsPage'
import { WordSetDetailPage } from './pages/WordSetDetailPage'
import { GamePage } from './pages/GamePage'
import { ChatPage } from './pages/ChatPage'

function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
      <Route element={<ProtectedRoute />}>
        <Route path="/" element={<WordSetsPage />} />
        <Route path="/wordsets/:id" element={<WordSetDetailPage />} />
        <Route path="/wordsets/:id/game" element={<GamePage />} />
        <Route path="/chat" element={<ChatPage />} />
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}

export default App
