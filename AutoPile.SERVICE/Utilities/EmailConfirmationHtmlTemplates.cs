using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.SERVICE.Utilities
{
    public static class EmailConfirmationHtmlTemplates
    {
        public static string GetEmailConfirmationHtml(bool isConfirmed) => isConfirmed
            ? SuccessTemplate
            : ErrorTemplate;

        private static string SuccessTemplate => @"
        <!DOCTYPE html>
        <html>
        <head>
            <title>Email Confirmation</title>
            <style>
                body {
                    margin: 0;
                    padding: 0;
                    min-height: 100vh;
                    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                    display: flex;
                    justify-content: center;
                    align-items: center;
                }
                .container {
                    background: rgba(255, 255, 255, 0.95);
                    padding: 2.5rem 3rem;
                    border-radius: 20px;
                    box-shadow: 0 10px 30px rgba(0, 0, 0, 0.1);
                    text-align: center;
                    max-width: 500px;
                    width: 90%;
                    transform: translateY(-20px);
                    animation: slideUp 0.5s ease forwards;
                }
                @keyframes slideUp {
                    to {
                        transform: translateY(0);
                        opacity: 1;
                    }
                }
                .icon {
                    width: 80px;
                    height: 80px;
                    background: #4CAF50;
                    border-radius: 50%;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    margin: 0 auto 1.5rem;
                }
                .icon svg {
                    width: 40px;
                    height: 40px;
                    fill: white;
                }
                h1 {
                    color: #2d3748;
                    margin: 0 0 1rem;
                    font-size: 1.8rem;
                    font-weight: 600;
                }
                p {
                    color: #718096;
                    line-height: 1.6;
                    margin: 0;
                    font-size: 1.1rem;
                }
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='icon'>
                    <svg viewBox='0 0 24 24'>
                        <path d='M9 16.17L4.83 12l-1.42 1.41L9 19 21 7l-1.41-1.41L9 16.17z'/>
                    </svg>
                </div>
                <h1>Email Verified!</h1>
                <p>Your email address has been successfully verified.</p>
                <p>You can now close this window and continue using AutoPile.</p>
            </div>
        </body>
        </html>";

        private static string ErrorTemplate => @"
        <!DOCTYPE html>
        <html>
        <head>
            <title>Email Confirmation Failed</title>
            <style>
                body {
                    margin: 0;
                    padding: 0;
                    min-height: 100vh;
                    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                    background: linear-gradient(135deg, #fc8181 0%, #f56565 100%);
                    display: flex;
                    justify-content: center;
                    align-items: center;
                }
                .container {
                    background: rgba(255, 255, 255, 0.95);
                    padding: 2.5rem 3rem;
                    border-radius: 20px;
                    box-shadow: 0 10px 30px rgba(0, 0, 0, 0.1);
                    text-align: center;
                    max-width: 450px;
                    width: 90%;
                    transform: translateY(-20px);
                    animation: slideUp 0.5s ease forwards;
                }
                @keyframes slideUp {
                    to {
                        transform: translateY(0);
                        opacity: 1;
                    }
                }
                .icon {
                    width: 80px;
                    height: 80px;
                    background: #f56565;
                    border-radius: 50%;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    margin: 0 auto 1.5rem;
                }
                .icon svg {
                    width: 40px;
                    height: 40px;
                    fill: white;
                }
                h1 {
                    color: #2d3748;
                    margin: 0 0 1rem;
                    font-size: 1.8rem;
                    font-weight: 600;
                }
                p {
                    color: #718096;
                    line-height: 1.6;
                    margin: 0;
                    font-size: 1.1rem;
                }
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='icon'>
                    <svg viewBox='0 0 24 24'>
                        <path d='M19 6.41L17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12 19 6.41z'/>
                    </svg>
                </div>
                <h1>Verification Failed</h1>
                <p>The verification link appears to be invalid or has expired.</p>
                <p>Please request a new verification email and try again.</p>
                <p>Alternatively, you might have already verified your email.</p>
            </div>
        </body>
        </html>";
    }
}