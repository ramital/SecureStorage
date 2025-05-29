import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import axios from 'axios'
import Navbar from '../components/Navbar'
import { API_URL } from '../utils/constants'

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
          <div className="form-header">
            <h1>Add New Patient</h1>
          </div>
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
              <div className="button-group">
                <button
                  className="group-button-secondary"
                  onClick={() => navigate('/patients')}
                >
                  Back
                </button>
                <button type="submit" className="group-button">
                  Add Patient
                </button>
              </div>
            </div>
          </form>
        </div>
      </div>
    </>
  )
}

export default AddPatient