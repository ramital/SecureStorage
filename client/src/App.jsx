import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import LoginPage from './pages/LoginPage'
import PatientList from './pages/PatientList'
import AddPatient from './pages/AddPatient'
import './App.css'

function PrivateRoute({ children }) {
  const token = localStorage.getItem('token')
  return token ? children : <Navigate to="/login" />
}

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/\" element={<Navigate to="/login\" replace />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/patients" element={
          <PrivateRoute>
            <PatientList />
          </PrivateRoute>
        } />
        <Route path="/add-patient" element={
          <PrivateRoute>
            <AddPatient />
          </PrivateRoute>
        } />
        <Route path="*" element={<Navigate to="/login\" replace />} />
      </Routes>
    </BrowserRouter>
  )
}

export default App