import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import axios from 'axios'
import Navbar from '../components/Navbar'
import { API_URL } from '../utils/constants'
import { calculateAge, capitalize } from '../utils/helpers'

function PatientList() {
  const [patients, setPatients] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [searchTerms, setSearchTerms] = useState({
    name: '',
    phone: '',
    city: '',
    age: ''
  })
  const navigate = useNavigate()

  const handleSearch = () => {
  }

  const filterPatients = (patients) => {
    return patients.filter(patient => {
      const identifiers = patient.Identifiers || {}
      const age = identifiers.dob ? calculateAge(identifiers.dob) : null
      
      const nameMatch = !searchTerms.name || 
        identifiers.fullName?.toLowerCase().includes(searchTerms.name.toLowerCase())
      const phoneMatch = !searchTerms.phone || 
        identifiers.phone?.toLowerCase().includes(searchTerms.phone.toLowerCase())
      const cityMatch = !searchTerms.city || 
        identifiers.city?.toLowerCase().includes(searchTerms.city.toLowerCase())
      const ageMatch = !searchTerms.age || 
        (age !== null && age.toString().includes(searchTerms.age))

      return nameMatch && phoneMatch && cityMatch && ageMatch
    })
  }

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
                <span   className="hr-style"/>
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

  const filteredPatients = filterPatients(patients)

  return (
    <>
      <Navbar />
      <div className="patient-container">
        <div className="patient-box">
          <div className="patient-header">
            <h1>Patient List</h1>
          </div>
          <div className="search-section">
            <div className="search-group">
              <span className="search-label">Name:</span>
              <input
                type="text"
                className="search-input"
                placeholder="Search by name..."
                value={searchTerms.name}
                onChange={(e) => setSearchTerms(prev => ({ ...prev, name: e.target.value }))}
              />
            </div>
            <div className="search-group">
              <span className="search-label">Phone:</span>
              <input
                type="text"
                className="search-input"
                placeholder="Search by phone..."
                value={searchTerms.phone}
                onChange={(e) => setSearchTerms(prev => ({ ...prev, phone: e.target.value }))}
              />
            </div>
            <div className="search-group">
              <span className="search-label">City:</span>
              <input
                type="text"
                className="search-input"
                placeholder="Search by city..."
                value={searchTerms.city}
                onChange={(e) => setSearchTerms(prev => ({ ...prev, city: e.target.value }))}
              />
            </div>
            <div className="search-group">
              <span className="search-label">Age:</span>
              <input
                type="text"
                className="search-input"
                placeholder="Search by age..."
                value={searchTerms.age}
                onChange={(e) => setSearchTerms(prev => ({ ...prev, age: e.target.value }))}
              />
            </div>
             
            <div className="search-group">
                <button
                  className="group-button-secondary"
                  onClick={() => navigate('/patient-consent')}
                >
                  Add New Patient
                </button>
              <button onClick={handleSearch} className="group-button">
                Search
              </button>
            </div>
          </div>
          <div className="patient-list">
            {filteredPatients.map(patient => (
              <div key={patient.UserId} className="patient-card">
                <RenderFields data={patient} exclude={["UserId"]} />
              </div>
            ))}
          </div>
        </div>
      </div>
    </>
  )
}

export default PatientList