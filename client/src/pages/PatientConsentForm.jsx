import { useState, useEffect } from 'react'
import { useParams, useSearchParams } from 'react-router-dom'
import { ClockIcon, CheckCircleIcon } from '@heroicons/react/24/outline'
import axios from 'axios'
import { API_URL } from '../utils/constants'

function PatientConsentForm() {
  const { patientId } = useParams()
  const [searchParams] = useSearchParams()
  const patientName = searchParams.get('name') || 'Patient'
  
  const [timeLeft, setTimeLeft] = useState(600) // 10 minutes in seconds
  const [consentAccepted, setConsentAccepted] = useState(false)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState('')
  
  // Checkbox states for each consent item
  const [checkboxes, setCheckboxes] = useState({
    identifiers: false,
    medicalRecords: false,
    contactInfo: false,
    insuranceInfo: false,
    financialInfo: false,
    biometricData: false
  })

  // Timer countdown effect - starts immediately when page loads
  useEffect(() => {
    if (timeLeft <= 0 || consentAccepted) {
      return
    }

    const timer = setInterval(() => {
      setTimeLeft(prev => {
        if (prev <= 1) {
          // Timer expired - could redirect or show expired message
          return 0
        }
        return prev - 1
      })
    }, 1000)

    return () => clearInterval(timer)
  }, [timeLeft, consentAccepted])

  // Format time as MM:SS
  const formatTime = (seconds) => {
    const minutes = Math.floor(seconds / 60)
    const remainingSeconds = seconds % 60
    return `${minutes.toString().padStart(2, '0')}:${remainingSeconds.toString().padStart(2, '0')}`
  }

  // Handle individual checkbox changes
  const handleCheckboxChange = (checkboxName) => {
    setCheckboxes(prev => ({
      ...prev,
      [checkboxName]: !prev[checkboxName]
    }))
  }

  // Handle "Accept All" button
  const handleAcceptAll = () => {
    const allChecked = Object.values(checkboxes).every(checked => checked)
    const newState = !allChecked // If all are checked, uncheck all; otherwise, check all
    
    setCheckboxes({
      identifiers: newState,
      medicalRecords: newState,
      contactInfo: newState,
      insuranceInfo: newState,
      financialInfo: newState,
      biometricData: newState
    })
  }

  // Check if mandatory checkbox (identifiers) is selected - THIS ENABLES THE SUBMIT BUTTON
  const isMandatoryAccepted = checkboxes.identifiers
  const allCheckboxesSelected = Object.values(checkboxes).every(checked => checked)

  const handleAcceptConsent = async () => {
    // This check is now redundant since button is disabled when identifiers is not checked
    if (!isMandatoryAccepted) {
      setError('Please accept the mandatory Identifiers consent term.')
      return
    }

    setIsSubmitting(true)
    setError('')

    try {
      // Prepare consent data for API submission
      const consentData = {
        patientId: patientId,
        patientName: patientName,
        consentTerms: checkboxes,
        timestamp: new Date().toISOString(),
      }

      // Submit consent to API
      await axios.post(`${API_URL}/consent`, consentData)
      
      console.log('Consent submitted successfully:', consentData)
      
      setConsentAccepted(true)
    } catch (err) {
      console.error('Failed to submit consent:', err)
      setError('Failed to submit consent. Please try again.')
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleDeclineConsent = () => {
    // Handle consent decline - could redirect or show decline message
    alert('Consent declined. You may close this window.')
  }

  // Show success page after consent is accepted
  if (consentAccepted) {
    return (
      <div className="consent-standalone-container">
        <div className="consent-standalone-box success">
          <h1 className="consent-success-title">Consent Accepted Successfully!</h1>
          <p className="consent-success-message">
            Thank you, <strong>{patientName}</strong>, for reviewing and accepting the consent form.
          </p>
          <p className="consent-success-submessage">
            Your healthcare provider has been notified and will proceed with your care.
          </p>
          <div className="consent-success-info">
            <p>You may now close this window.</p>
          </div>
        </div>
      </div>
    )
  }

  // Show expired page if timer runs out
  if (timeLeft <= 0) {
    return (
      <div className="consent-standalone-container">
        <div className="consent-standalone-box expired">
          <h1 className="consent-expired-title">Consent Form Expired</h1>
          <p className="consent-expired-message">
            The consent form for <strong>{patientName}</strong> has expired.
          </p>
          <p className="consent-expired-submessage">
            Please contact your healthcare provider to request a new consent form.
          </p>
        </div>
      </div>
    )
  }

  return (
    <div className="consent-standalone-container">
      <div className="consent-standalone-box">
        <div className="consent-standalone-header">
          <h1>Patient Consent Form</h1>
          <div className={`timer-standalone ${timeLeft <= 60 ? 'urgent' : ''}`}>
            <ClockIcon className="w-5 h-5" />
            <span>{formatTime(timeLeft)}</span>
          </div>
        </div>

        <div className="patient-info-section">
          <h2>Patient: <strong>{patientName}</strong></h2>
        </div>

        <div className="consent-content">
          <h2>Patient Consent to Share Health Information</h2>
          
          <p>
            By agreeing to this consent, I authorize the secure use and sharing of my personal and medical 
            information for treatment, billing, and healthcare operations. Data will only be accessed by 
            relevant personnel based on their role and necessity.
          </p>

          {/* Consent Checkboxes Section */}
          <div className="consent-checkboxes">
            <h3>Please review and accept each consent term:</h3>
            
            <div className="checkbox-item" style={{ backgroundColor: '#fef3c7', border: '2px solid #f59e0b' }}>
              <input
                type="checkbox"
                id="identifiers"
                checked={checkboxes.identifiers}
                onChange={() => handleCheckboxChange('identifiers')}
              />
              <label htmlFor="identifiers">
                <strong>Identifiers (MANDATORY)</strong> (e.g., name, DOB, SSN): accessed by doctors, nurses, receptionists, 
                billing staff, and admins for patient identification and coordination.
              </label>
            </div>

            <div className="checkbox-item">
              <input
                type="checkbox"
                id="medicalRecords"
                checked={checkboxes.medicalRecords}
                onChange={() => handleCheckboxChange('medicalRecords')}
              />
              <label htmlFor="medicalRecords">
                <strong>Medical Records (Optional)</strong> (e.g., diagnoses, medications, lab results): accessed by doctors 
                and nurses for care delivery, lab technicians for test processing, and admins for audits.
              </label>
            </div>

            <div className="checkbox-item">
              <input
                type="checkbox"
                id="contactInfo"
                checked={checkboxes.contactInfo}
                onChange={() => handleCheckboxChange('contactInfo')}
              />
              <label htmlFor="contactInfo">
                <strong>Contact Info (Optional)</strong> (e.g., phone, email, address): used by clinical staff and reception 
                for communication and follow-up.
              </label>
            </div>

            <div className="checkbox-item">
              <input
                type="checkbox"
                id="insuranceInfo"
                checked={checkboxes.insuranceInfo}
                onChange={() => handleCheckboxChange('insuranceInfo')}
              />
              <label htmlFor="insuranceInfo">
                <strong>Insurance Info (Optional)</strong> (e.g., policy, coverage): accessed by billing staff, receptionists, 
                and insurers to verify eligibility and process claims.
              </label>
            </div>

            <div className="checkbox-item">
              <input
                type="checkbox"
                id="financialInfo"
                checked={checkboxes.financialInfo}
                onChange={() => handleCheckboxChange('financialInfo')}
              />
              <label htmlFor="financialInfo">
                <strong>Financial Info (Optional)</strong> (e.g., payments, billing): used by billing staff and admins for 
                transactions and compliance.
              </label>
            </div>

            <div className="checkbox-item">
              <input
                type="checkbox"
                id="biometricData"
                checked={checkboxes.biometricData}
                onChange={() => handleCheckboxChange('biometricData')}
              />
              <label htmlFor="biometricData">
                <strong>Biometric Data (Optional)</strong> (e.g., fingerprint, face scan): used for authentication by clinical 
                and admin staff if applicable.
              </label>
            </div>
          </div>

          {/* Accept All Button */}
          <div className="accept-all-section">
            <button
              type="button"
              className="accept-all-button"
              onClick={handleAcceptAll}
            >
              {allCheckboxesSelected ? 'Uncheck All' : 'Accept All Terms'}
            </button>
          </div>

          <div className="consent-terms">
            <p>
              This consent remains in effect until revoked in writing. Revocation does not affect prior use.
            </p>
          </div>

          <div className="signature-section">
            <p><strong>Signature:</strong> _____________________</p>
            <p><strong>Date:</strong> {new Date().toLocaleDateString()}</p>
          </div>
        </div>

        {error && <div className="error-message">{error}</div>}

        <div className="consent-actions">
          <button
            type="button"
            className="decline-button"
            onClick={handleDeclineConsent}
            disabled={isSubmitting}
          >
            Decline
          </button>
          <button
            type="button"
            className="accept-button"
            onClick={handleAcceptConsent}
            disabled={isSubmitting || !isMandatoryAccepted}
          >
            {isSubmitting ? 'Submitting...' : 'Accept & Sign'}
          </button>
        </div>

        <div className="consent-footer">
          <p className="timer-warning">
            This form will expire in {formatTime(timeLeft)}. Please review and respond before the timer expires.
          </p>
        </div>
      </div>
    </div>
  )
}

export default PatientConsentForm