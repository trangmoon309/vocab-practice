import { Navigate, Route, Routes } from 'react-router-dom'
import { ProtectedRoute } from './components/layout/ProtectedRoute'
import { ChatWidget } from './components/chat/ChatWidget'
import { LoginPage } from './pages/LoginPage'
import { RegisterPage } from './pages/RegisterPage'
import { WordsPage } from './pages/WordsPage'
import { GameSelectPage } from './pages/GameSelectPage'
import { GamePage } from './pages/GamePage'

function App() {
  return (
    <>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route element={<ProtectedRoute />}>
          <Route path="/" element={<WordsPage />} />
          <Route path="/game" element={<GameSelectPage />} />
          <Route path="/game/play" element={<GamePage />} />
        </Route>
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
      <ChatWidget />
    </>
  )
}

export default App
