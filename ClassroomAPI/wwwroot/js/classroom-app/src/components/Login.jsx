import React, { useState } from 'react'
import { data, NavLink } from 'react-router-dom';

function Login({setAuthToken, setShowRegister}){

    document.title = 'Login: Assignment-App'

    const [formData, setFormData] = useState({
        userName: '',
        password: '',
    })

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData((prevData) => ({...prevData, [name]: value}))
    }

    const handleSubmit =  async (e) => {
        e.preventDefault()

         // Password validation regex
         const passwordRegex =
         /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$/;

        if (!passwordRegex.test(formData.password)) {
            alert(
                'Password must contain at least one uppercase letter, one lowercase letter, one number, one special character, and be at least 6 characters long.'
            );
            return;
        }

        try {
            const response = await fetch('https://localhost:7110/api/account/login', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(formData),
                credentials: 'include',
            })

            if(response.ok){
                const data = await response.json()
                setAuthToken(data.token)
                {<NavLink to="/courses" />}
                // setTimeout(() => {
                //     alert("Login successful!")
                // }, 1000);
            }
            else{
                const errorData = await response.json();
                alert('Error: ' + errorData);
            }
        }

        catch (error) {
            alert('Error: Wrong username or password')
            console.error('Error during login:', error);
        }
    } 

    return (
    <div className='border border-warning-subtle' style={styles.container}>
        <h2 style={styles.heading}>Login</h2>
        <form style={styles.form} onSubmit={handleSubmit}>
            <input style={styles.input} type='text' name='userName' placeholder='Username' value={formData.userName} onChange={handleChange}/>
            <input style={styles.input} type='password' name='password' placeholder='Password' value={formData.password} onChange={handleChange}/>
            <button style={styles.button} type='submit'>Login</button>
        </form>
        <p style={styles.footerText}>Not a member?  <button onClick={() => setShowRegister(true)} style={styles.linkButton} >Register</button></p>
        
    </div>
    );

}

const styles = {
    container: { maxWidth: '400px', margin: 'auto', padding: '20px', textAlign: 'center', fontFamily: 'Arial, sans-serif', marginTop: 100 },
    heading: { marginBottom: '20px' },
    form: { display: 'flex', flexDirection: 'column', gap: '10px' },
    input: { padding: '10px', fontSize: '16px', borderRadius: '4px', border: '1px solid #ccc' },
    button: { padding: '10px', fontSize: '16px', backgroundColor: '#28A745', color: '#fff', border: 'none', borderRadius: '4px', cursor: 'pointer' },
    footerText: { marginTop: '20px' },
    linkButton: { color: '#28A745', textDecoration: 'underline', background: 'none', border: 'none', cursor: 'pointer' },
  };

export default Login;