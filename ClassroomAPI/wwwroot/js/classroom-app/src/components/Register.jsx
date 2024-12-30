import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { NavLink } from 'react-router-dom';
import '../styles/Register.css';

function Register({setShowRegister}){

    document.title = 'Register: Assignment-App'

    const [formData, setFormData] = useState({
        userName: '',
        email: '',
        password: '',
        fullName: '',
        phoneNumber: '',
    })

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData((prevData) => ({ ...prevData, [name]: value}));
    }

    const handleSubmit = async (e) => {
        e.preventDefault();

         // Password validation regex
         const passwordRegex =
         /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$/;

        if (!passwordRegex.test(formData.password)) {
            alert(
                'Password must contain at least one uppercase letter, one lowercase letter, one number, one special character, and be at least 6 characters long.'
            );
            return;
        }

        try{
            const response = await fetch('https://localhost:7110/api/account/register', {
                method:'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(formData),
                credentials: 'include',
            });

            if(response.ok){
                // setTimeout(() => {
                //     alert('Registration successful! Redirecting to login...');
                // }, 1000);
                
                setShowRegister(false);
            }
            else{
                const errorData = await response.json();
                alert('Error: ' + errorData);
            }
        }

        catch (error) {
            console.error('Error during registration:', error);
        }
    }

    return (
        /* From Uiverse.io by ammarsaa */ 
        <div className="container" style={{marginLeft: '35%', marginTop: 100}}>
            <form className="form" onSubmit={handleSubmit}>
                <p className="title">Register </p>
                <p className="message">Signup now and get full access to our app. </p>
                    <div className="flex">
                    <label>
                        <input className="input" name='fullName' type="text" onChange={handleChange} placeholder="FullName"  value={formData.fullName} required=""/>
                        <span>Full Name</span>
                    </label>

                    <label>
                        <input className="input" name='userName' type="text" onChange={handleChange} placeholder="Username" value={formData.userName} required=""/>
                        <span>UserName</span>
                    </label>
                </div>  
                        
                <label>
                    <input className="input"  name='email' type="email" onChange={handleChange} placeholder="Email" value={formData.email}/>
                    <span>Email</span>
                </label> 
                    
                <label>
                    <input className="input" name='password' type="password" onChange={handleChange} placeholder="Password"  value={formData.password}/>
                    <span>Password</span>
                </label>
                <button className="submit">Submit</button>
                <p className="signin">Already have an acount ? <a type='button' onClick={() => setShowRegister(false)}>Signin</a> </p>
            </form>
        </div>
    );
}


export default Register;