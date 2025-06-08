import { useState, useEffect } from 'react'
import { useNavigate, useLocation } from 'react-router-dom'
import { PhoneIcon, ClockIcon, CheckCircleIcon, EnvelopeIcon } from '@heroicons/react/24/outline'
import { v4 as uuidv4 } from 'uuid'
import axios from 'axios'
import Navbar from '../components/Navbar'
import { API_URL } from '../utils/constants'

function PatientConsent() {
  const navigate = useNavigate()
  const location = useLocation()
  const initialPatientName = location.state?.patientName || ''
  
  const [patientName, setPatientName] = useState(initialPatientName)
  const [contactMethod, setContactMethod] = useState('phone') // 'phone' or 'email'
  const [phoneNumber, setPhoneNumber] = useState('')
  const [email, setEmail] = useState('')
  const [timeLeft, setTimeLeft] = useState(null) // Start as null, will be set to 600 after sending
  const [isConsentSent, setIsConsentSent] = useState(false)
  const [isSending, setIsSending] = useState(false)
  const [error, setError] = useState('')
  const [timerStarted, setTimerStarted] = useState(false)
  const [patientId, setPatientId] = useState(null) // Store patient ID for checking consent
  const [consentAccepted, setConsentAccepted] = useState(false)
  const [isCheckingConsent, setIsCheckingConsent] = useState(false)
  const [consentLink, setConsentLink] = useState('')

  // Timer countdown effect - only starts after consent is sent
  useEffect(() => {
    if (!timerStarted || timeLeft === null || timeLeft <= 0) {
      if (timeLeft === 0) {
        navigate('/patients')
      }
      return
    }

    const timer = setInterval(() => {
      setTimeLeft(prev => prev - 1)
    }, 1000)

    return () => clearInterval(timer)
  }, [timeLeft, timerStarted, navigate])

  // API-based consent checker - runs every 30 seconds during the timer period
  useEffect(() => {
    if (!timerStarted || !patientId || consentAccepted) {
      return
    }

    const checkConsentStatus = async () => {
      try {
        setIsCheckingConsent(true)
        
        // Get authentication token
        const token = localStorage.getItem('token')
        if (!token) {
          console.error('No authentication token found')
          return
        }

        // Make API call to check consent status using patient GUID
        const response = await axios.get(`${API_URL}/consent/${patientId}`, {
          headers: { 
            Authorization: `Bearer ${token}`,
            'Content-Type': 'application/json'
          }
        })

        console.log('Consent status check response:', response.data)

        // Check if consent has been accepted
        if (response.data != null || response.data != undefined ) {
          console.log('Consent accepted via API check!')
          setConsentAccepted(true)
          setTimeLeft(null) // Stop the timer
          setTimerStarted(false)
          
          // Automatically navigate to Add Patient page with patient name and GUID
          setTimeout(() => {
            navigate('/add-patient', { 
              state: { 
                patientName: patientName,
                fromConsent: true,
                patientId: patientId // Pass the GUID to AddPatient
              } 
            })
          }, 2000) // Wait 2 seconds to show success message briefly
        }
        
      } catch (err) {
        console.error('Failed to check consent status:', err)
        
        // Handle different error scenarios
        if (err.response?.status === 401) {
          console.error('Authentication failed - redirecting to login')
          navigate('/login')
        } else if (err.response?.status === 404) {
          console.log('Consent record not found yet - patient may not have responded')
        } else {
          console.error('API error checking consent status:', err.response?.data || err.message)
        }
      } finally {
        setIsCheckingConsent(false)
      }
    }

    // Check immediately when timer starts, then every 30 seconds
    checkConsentStatus()
    const checkInterval = setInterval(checkConsentStatus, 10000) // 30 seconds

    return () => clearInterval(checkInterval)
  }, [timerStarted, patientId, consentAccepted, navigate, patientName])

  // Format time as MM:SS
  const formatTime = (seconds) => {
    if (seconds === null) return '--:--'
    const minutes = Math.floor(seconds / 60)
    const remainingSeconds = seconds % 60
    return `${minutes.toString().padStart(2, '0')}:${remainingSeconds.toString().padStart(2, '0')}`
  }

  // Format phone number as user types
  const formatPhoneNumber = (value) => {
    const phoneNumber = value.replace(/[^\d]/g, '')
    const phoneNumberLength = phoneNumber.length
    
    if (phoneNumberLength < 4) return phoneNumber
    if (phoneNumberLength < 7) {
      return `(${phoneNumber.slice(0, 3)}) ${phoneNumber.slice(3)}`
    }
    return `(${phoneNumber.slice(0, 3)}) ${phoneNumber.slice(3, 6)}-${phoneNumber.slice(6, 10)}`
  }

  const handlePhoneChange = (e) => {
    const formatted = formatPhoneNumber(e.target.value)
    setPhoneNumber(formatted)
  }

  const handleEmailChange = (e) => {
    setEmail(e.target.value)
  }

  const handlePatientNameChange = (e) => {
    setPatientName(e.target.value)
  }

  const validateEmail = (email) => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
    return emailRegex.test(email)
  }

  const handleSendConsent = async () => {
    setError('')

    // Validate patient name
    if (!patientName.trim()) {
      setError('Please enter the patient name')
      return
    }

    if (contactMethod === 'phone') {
      if (!phoneNumber || phoneNumber.length < 14) {
        setError('Please enter a valid phone number')
        return
      }
    } else {
      if (!email || !validateEmail(email)) {
        setError('Please enter a valid email address')
        return
      }
    }

    setIsSending(true)

    try {
      // Simulate sending (no actual API call)
      await new Promise(resolve => setTimeout(resolve, 1000)) // 1 second delay for UX
      
      // Generate a unique patient ID using UUID
      const generatedPatientId = uuidv4()
      setPatientId(generatedPatientId)
      
      // Create the consent link
      const baseUrl = window.location.origin
      const encodedPatientName = encodeURIComponent(patientName.trim())
      const link = `${baseUrl}/consent/${generatedPatientId}?name=${encodedPatientName}`
      setConsentLink(link)
      
      console.log('Consent link generated:', link)
      console.log('Patient ID (UUID):', generatedPatientId)
      
      // Start the timer after successful "sending"
      setTimeLeft(600) // 10 minutes
      setTimerStarted(true)
      setIsConsentSent(true)
      
    } catch (err) {
      console.error('Failed to send consent:', err)
      setError(`Failed to send consent via ${contactMethod === 'phone' ? 'SMS' : 'email'}. Please try again.`)
    } finally {
      setIsSending(false)
    }
  }

  const handleSkip = () => {
    navigate('/patients')
  }

  const getContactValue = () => {
    return contactMethod === 'phone' ? phoneNumber : email
  }

  const isFormValid = () => {
    const hasPatientName = patientName.trim().length > 0
    if (contactMethod === 'phone') {
      return hasPatientName && phoneNumber && phoneNumber.length >= 14
    } else {
      return hasPatientName && email && validateEmail(email)
    }
  }

  const copyToClipboard = async (text) => {
    try {
      await navigator.clipboard.writeText(text)
      alert('Link copied to clipboard!')
    } catch (err) {
      console.error('Failed to copy:', err)
      // Fallback for older browsers
      const textArea = document.createElement('textarea')
      textArea.value = text
      document.body.appendChild(textArea)
      textArea.select()
      document.execCommand('copy')
      document.body.removeChild(textArea)
      alert('Link copied to clipboard!')
    }
  }

  // Show consent accepted success page (briefly before auto-navigation)
  if (consentAccepted) {
    return (
      <>
        <Navbar />
        <div className="consent-container">
          <div className="consent-box success">
            <div className="consent-header">
              <h1>Consent Accepted!</h1>
            </div>
            <p className="text-xl font-semibold text-green-600 mb-4">
              Patient consent has been successfully accepted!
            </p>
            <p><strong>{patientName}</strong> has reviewed and signed the consent form.</p>
            <p>Contact: <strong>{getContactValue()}</strong></p>
           
            
            <div className="consent-info">
              <p>
                The consent process is now complete. Redirecting to Add Patient page...
              </p>
            </div>

            <div className="redirect-message">
              <p>You will be automatically redirected to add the patient details.</p>
            </div>
          </div>
        </div>
      </>
    )
  }

  if (isConsentSent) {
    return (
      <>
        <Navbar />
        <div className="consent-container">
          <div className="consent-box success">
            <div className="consent-header">
              <h1>Consent Sent Successfully!</h1>
              <div className="timer-display">
                <ClockIcon className="w-5 h-5" />
                <span className={`timer-text ${timeLeft <= 60 ? 'urgent' : ''}`}>
                  {formatTime(timeLeft)}
                </span>
              </div>
            </div>
            <p>Patient consent has been sent to <strong>{patientName}</strong>:</p>
            <p><strong>{getContactValue()}</strong></p>
            <p>via {contactMethod === 'phone' ? 'SMS' : 'Email'}</p>
            
            {/* Display the consent link */}
            <div className="consent-link-section" style={{ 
              background: '#f0f9ff', 
              padding: '1.5rem', 
              borderRadius: '8px', 
              margin: '1.5rem 0',
              border: '2px solid #3b82f6'
            }}>
              <h3 style={{ margin: '0 0 1rem 0', color: '#1e40af', fontSize: '1.1rem', fontWeight: '600' }}>
                Patient Consent Link:
              </h3>
              <div style={{ 
                background: 'white', 
                padding: '1rem', 
                borderRadius: '4px', 
                border: '1px solid #e2e8f0',
                wordBreak: 'break-all',
                fontSize: '0.9rem',
                color: '#374151',
                marginBottom: '1rem'
              }}>
                {consentLink}
              </div>
              <button
                onClick={() => copyToClipboard(consentLink)}
                style={{
                  background: '#3b82f6',
                  color: 'white',
                  border: 'none',
                  padding: '0.5rem 1rem',
                  borderRadius: '4px',
                  cursor: 'pointer',
                  fontSize: '0.9rem',
                  fontWeight: '500'
                }}
                onMouseOver={(e) => e.target.style.background = '#2563eb'}
                onMouseOut={(e) => e.target.style.background = '#3b82f6'}
              >
                Copy Link
              </button>
            </div>
            
            <div className="consent-info">
              <p>
                The patient will receive a {contactMethod === 'phone' ? 'text message' : 'email'} with 
                the above link to review and sign the consent form.
              </p>
              <p>Session will automatically end in {formatTime(timeLeft)}.</p>
              {isCheckingConsent && (
                <p className="text-blue-600">
                  🔄 Checking for consent acceptance via API...
                </p>
              )}
              <p className="text-sm text-gray-600 mt-2">
                This page will automatically redirect to Add Patient when consent is accepted.
              </p>
            </div>

            <div className="consent-actions">
              <button
                type="button"
                className="group-button"
                onClick={() => navigate('/patients')}
              >
                Return to Patient List
              </button>
            </div>
          </div>
        </div>
      </>
    )
  }

  return (
    <>
      <Navbar />
      <div className="consent-container">
        <div className="consent-box">
          <div className="consent-header">
            <h1>Send Patient Consent</h1>
            {/* Timer is now completely hidden until after sending */}
          </div>
          
          {error && <div className="error-message">{error}</div>}

          {/* Patient Name Input */}
          <div className="contact-input-section">
            <div className="form-group">
              <label htmlFor="patientName">Patient Name</label>
              <input
                type="text"
                id="patientName"
                value={patientName}
                onChange={handlePatientNameChange}
                placeholder="Enter patient full name"
                className="contact-input"
                required
              />
            </div>
          </div>

          {/* Contact Method Selection */}
          <div className="contact-method-section">
            <label className="section-label">Choose Contact Method:</label>
            <div className="contact-method-options">
              <label className="contact-method-option">
                <input
                  type="radio"
                  name="contactMethod"
                  value="phone"
                  checked={contactMethod === 'phone'}
                  onChange={(e) => setContactMethod(e.target.value)}
                />
                <PhoneIcon className="w-5 h-5" />
                <span>SMS / Text Message</span>
              </label>
              <label className="contact-method-option">
                <input
                  type="radio"
                  name="contactMethod"
                  value="email"
                  checked={contactMethod === 'email'}
                  onChange={(e) => setContactMethod(e.target.value)}
                />
                <EnvelopeIcon className="w-5 h-5" />
                <span>Email</span>
              </label>
            </div>
          </div>

          {/* Contact Input Section */}
          <div className="contact-input-section">
            {contactMethod === 'phone' ? (
              <div className="form-group">
                <label htmlFor="phone">Patient Phone Number</label>
                <input
                  type="tel"
                  id="phone"
                  value={phoneNumber}
                  onChange={handlePhoneChange}
                  placeholder="(555) 123-4567"
                  maxLength={14}
                  className="contact-input"
                />
              </div>
            ) : (
              <div className="form-group">
                <label htmlFor="email">Patient Email Address</label>
                <input
                  type="email"
                  id="email"
                  value={email}
                  onChange={handleEmailChange}
                  placeholder="patient@example.com"
                  className="contact-input"
                />
              </div>
            )}
          </div>

          <div className="consent-actions">
            <button
              type="button"
              className="send-consent-button"
              onClick={handleSendConsent}
              disabled={isSending || !isFormValid()}
            >
              {isSending ? 'Sending...' : `Send via ${contactMethod === 'phone' ? 'SMS' : 'Email'}`}
            </button>
          </div>

          <div className="consent-info">
            <p>
              The patient will receive a {contactMethod === 'phone' ? 'text message' : 'email'} with 
              a link to review and sign the consent form.
            </p>
            <p>A 10-minute timer will start after sending the consent.</p>
          </div>
        </div>
      </div>
    </>
  )
}

export default PatientConsent