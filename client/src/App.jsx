import { useState, useEffect, Fragment } from 'react'
import { BrowserRouter, Routes, Route, Navigate, useNavigate, Link } from 'react-router-dom'
import axios from 'axios'
import './App.css'

const API_URL = 'https://localhost:32773/api'

axios.interceptors.response.use(
  response => response,
  error => {
    if (error.response && error.response.status === 401) {
      localStorage.removeItem('token')
      window.location.href = '/login'
    }
    return Promise.reject(error)
  }
)

function Navbar() {
  const navigate = useNavigate()

  const handleLogout = () => {
    localStorage.removeItem('token')
    navigate('/login')
  }

  return (
    <nav className="navbar">
      <div className="navbar-content">
        <Link to="/patients" className="navbar-brand">
          <img src="src/assets/logo.png" 
               alt="Health Icon"  height={70}
               className="navbar-logo" />
          Patient Management System
        </Link>
        <div className="navbar-actions">
          <button onClick={handleLogout} className="logout-button">
            Logout
          </button>
        </div>
      </div>
    </nav>
  )
}

function LoginPage() {
  const navigate = useNavigate()
  const [formData, setFormData] = useState({
    username: '',
    password: ''
  })
  const [error, setError] = useState('')

  const handleSubmit = async (e) => {
    e.preventDefault()
    try {
      const response = await axios.post(`${API_URL}/Auth/login`, formData)
      localStorage.setItem('token', response.data)
      navigate('/patients')
    } catch (err) {
      setError('Invalid credentials')
    }
  }

  const handleChange = (e) => {
    const { name, value } = e.target
    setFormData(prev => ({
      ...prev,
      [name]: value
    }))
  }

  return (
    <div className="login-container">
      <div className="login-box">
        <img src="src/assets/logo.png" 
             alt="Health Icon" 
             className="login-logo" />
        <h1>Welcome Back</h1>
        {error && <p className="error-message">{error}</p>}
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="username">Username</label>
            <input
              type="text"
              id="username"
              name="username"
              value={formData.username}
              onChange={handleChange}
              placeholder="Username"
              required
            />
          </div>
          <div className="form-group">
            <label htmlFor="password">Password</label>
            <input
              type="password"
              id="password"
              name="password"
              value={formData.password}
              onChange={handleChange}
              placeholder="Password"
              required
            />
          </div>
          <button type="submit" className="login-button">
            Log In
          </button>
        </form>
      </div>
    </div>
  )
}

function PrivateRoute({ children }) {
  const token = localStorage.getItem('token')
  return token ? children : <Navigate to="/login" />
}

function PatientList() {
  const [patients, setPatients] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const navigate = useNavigate()

  const capitalize = (s) => s.charAt(0).toUpperCase() + s.slice(1)

  const RenderFields = ({ data, exclude = [] }) => {
    const entries = Object.entries(data)
      .filter(([k]) => !exclude.includes(k))
      .sort(([a], [b]) => {
        if (a === "Identifiers") return -1
        if (b === "Identifiers") return 1
        return a.localeCompare(b)
      })

    return entries.map(([key, val]) =>
      typeof val === "object" ? (
        <div key={key} className="mb-4">
          <h4 className="text-lg font-semibold text-gray-800 mb-2">
            {capitalize(key)}
          </h4>
          <div className="pl-4 border-l-2 border-gray-200">
            <RenderFields data={val} exclude={[]} />
          </div>
        </div>
      ) : (
        <div key={key} className="patient-info-item">
          <span className="info-label">{capitalize(key)}:</span>
          <span className="info-value">{val}</span>
        </div>
      )
    )
  }

  useEffect(() => {
    const fetchPatients = async () => {
      try {
        const token = localStorage.getItem("token")
        if (!token) {
          navigate('/login')
          return
        }

        const { data } = await axios.get(`${API_URL}/patients`, {
          headers: { Authorization: `Bearer ${token}` },
        })

        let arr
        if (Array.isArray(data)) {
          arr = data
        } else if (Array.isArray(data.patients)) {
          arr = data.patients
        } else {
          arr = Object.values(data)
        }

        setPatients(arr)
      } catch (err) {
        console.error(err)
        if (err.response?.status === 401) {
          navigate('/login')
        } else {
          setError("Failed to fetch patients")
        }
      } finally {
        setLoading(false)
      }
    }

    fetchPatients()
  }, [navigate])

  if (loading) return (
    <>
      <Navbar />
      <div className="loading-spinner">Loading patients...</div>
    </>
  )

  if (error) return (
    <>
      <Navbar />
      <div className="error-message">{error}</div>
    </>
  )

  return (
    <>
      <Navbar />
      <div className="patient-container">
        <div className="patient-header">
          <h1>Patient List</h1>
          <button
            className="add-patient-button"
            onClick={() => navigate('/add-patient')}
          >
            Add New Patient
          </button>
        </div>
        <div className="patient-list">
          {patients.map(patient => (
            <div key={patient.UserId} className="patient-card">
              <RenderFields data={patient} exclude={["UserId"]} />
            </div>
          ))}
        </div>
      </div>
    </>
  )
}

function AddPatient() {
  const navigate = useNavigate()
  const [formData, setFormData] = useState({
    Identifiers: {
      fullName: '',
      dob: '',
      ssn: '',
      mrn: ''
    },
    InsuranceInfo: {
      policyNumber: '',
      provider: '',
      coverage: ''
    },
    BiometricData: {
      fingerprintHash: '',
      retinaScan: '',
      faceMap: ''
    }
  })
  const [error, setError] = useState('')

  const handleChange = (section, field, value) => {
    setFormData(prev => ({
      ...prev,
      [section]: {
        ...prev[section],
        [field]: value
      }
    }))
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    try {
      const token = localStorage.getItem('token')
      if (!token) {
        navigate('/login')
        return
      }

      await axios.post(`${API_URL}/patients`, formData, {
        headers: { Authorization: `Bearer ${token}` }
      })
      navigate('/patients')
    } catch (err) {
      if (err.response?.status === 401) {
        navigate('/login')
      } else {
        setError('Failed to add patient')
      }
    }
  }

  return (
    <>
      <Navbar />
      <div className="add-patient-container">
        <div className="add-patient-box">
          <h1>Add New Patient</h1>
          {error && <p className="error-message">{error}</p>}
          <form onSubmit={handleSubmit}>
            <h2 className="section-title">Personal Information</h2>
            <div className="form-section">
              <div className="form-group">
                <label htmlFor="fullName">Full Name</label>
                <input
                  type="text"
                  id="fullName"
                  value={formData.Identifiers.fullName}
                  onChange={(e) => handleChange('Identifiers', 'fullName', e.target.value)}
                  placeholder="Enter full name"
                />
              </div>
              <div className="form-group">
                <label htmlFor="dob">Date of Birth</label>
                <input
                  type="date"
                  id="dob"
                  value={formData.Identifiers.dob}
                  onChange={(e) => handleChange('Identifiers', 'dob', e.target.value)}
                />
              </div>
              <div className="form-group">
                <label htmlFor="ssn">SSN</label>
                <input
                  type="text"
                  id="ssn"
                  value={formData.Identifiers.ssn}
                  onChange={(e) => handleChange('Identifiers', 'ssn', e.target.value)}
                  placeholder="XXX-XX-XXXX"
                />
              </div>
              <div className="form-group">
                <label htmlFor="mrn">Medical Record Number</label>
                <input
                  type="text"
                  id="mrn"
                  value={formData.Identifiers.mrn}
                  onChange={(e) => handleChange('Identifiers', 'mrn', e.target.value)}
                  placeholder="Enter MRN"
                />
              </div>
            </div>

            <h2 className="section-title">Insurance Information</h2>
            <div className="form-section">
              <div className="form-group">
                <label htmlFor="policyNumber">Policy Number</label>
                <input
                  type="text"
                  id="policyNumber"
                  value={formData.InsuranceInfo.policyNumber}
                  onChange={(e) => handleChange('InsuranceInfo', 'policyNumber', e.target.value)}
                  placeholder="Enter policy number"
                />
              </div>
              <div className="form-group">
                <label htmlFor="provider">Insurance Provider</label>
                <input
                  type="text"
                  id="provider"
                  value={formData.InsuranceInfo.provider}
                  onChange={(e) => handleChange('InsuranceInfo', 'provider', e.target.value)}
                  placeholder="Enter insurance provider"
                />
              </div>
              <div className="form-group">
                <label htmlFor="coverage">Coverage Details</label>
                <input
                  type="text"
                  id="coverage"
                  value={formData.InsuranceInfo.coverage}
                  onChange={(e) => handleChange('InsuranceInfo', 'coverage', e.target.value)}
                  placeholder="Enter coverage details"
                />
              </div>
            </div>

            <h2 className="section-title">Biometric Data</h2>
            <div className="form-section">
              <div className="form-group">
                <label htmlFor="fingerprintHash">Fingerprint Hash</label>
                <input
                  type="text"
                  id="fingerprintHash"
                  value={formData.BiometricData.fingerprintHash}
                  onChange={(e) => handleChange('BiometricData', 'fingerprintHash', e.target.value)}
                  placeholder="Enter fingerprint hash"
                />
              </div>
              <div className="form-group">
                <label htmlFor="retinaScan">Retina Scan</label>
                <input
                  type="text"
                  id="retinaScan"
                  value={formData.BiometricData.retinaScan}
                  onChange={(e) => handleChange('BiometricData', 'retinaScan', e.target.value)}
                  placeholder="Enter retina scan data"
                />
              </div>
              <div className="form-group">
                <label htmlFor="faceMap">Face Map</label>
                <input
                  type="text"
                  id="faceMap"
                  value={formData.BiometricData.faceMap}
                  onChange={(e) => handleChange('BiometricData', 'faceMap', e.target.value)}
                  placeholder="Enter face map data"
                />
              </div>
            </div>
        
            <button type="submit" className="submit-button">Add Patient</button>
          </form>
        </div>
      </div>
    </>
  )
}

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Navigate to="/login" />} />
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
      </Routes>
    </BrowserRouter>
  )
}

export default App