import { useState, useEffect, useRef } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import { UserCircleIcon, ArrowRightOnRectangleIcon } from '@heroicons/react/24/outline'
import { capitalize } from '../utils/helpers'

function Navbar() {
  const navigate = useNavigate()
  const [isDropdownOpen, setIsDropdownOpen] = useState(false)
  const dropdownRef = useRef(null)
  const user = capitalize(localStorage.getItem('username') || 'User')

  const handleLogout = () => {
    localStorage.removeItem('token')
    localStorage.removeItem('username')
    navigate('/login')
  }

  useEffect(() => {
    function handleClickOutside(event) {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target)) {
        setIsDropdownOpen(false)
      }
    }

    document.addEventListener('mousedown', handleClickOutside)
    return () => {
      document.removeEventListener('mousedown', handleClickOutside)
    }
  }, [])

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
          <div className="username-dropdown" ref={dropdownRef}>
            <div 
              className="username-display"
              onClick={() => setIsDropdownOpen(!isDropdownOpen)}
            >
              <UserCircleIcon className="user-icon" />
              {user}  
            </div>
            <div className={`dropdown-content ${isDropdownOpen ? 'show' : ''}`}>
              <div className="dropdown-item" onClick={handleLogout}>
                <ArrowRightOnRectangleIcon className="logout-icon" />
                Logout
              </div>
            </div>
          </div>
        </div>
      </div>
    </nav>
  )
}

export default Navbar